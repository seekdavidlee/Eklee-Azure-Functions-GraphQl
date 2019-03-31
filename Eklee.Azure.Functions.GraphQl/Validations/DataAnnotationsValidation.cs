using FastMember;
using GraphQL;
using GraphQL.Language.AST;
using GraphQL.Validation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Eklee.Azure.Functions.GraphQl.Validations
{
	public class DataAnnotationsValidation : IValidationRule
	{
		private readonly IEnumerable<IModelValidation> _modelValidations;

		public DataAnnotationsValidation(IEnumerable<IModelValidation> modelValidations)
		{
			_modelValidations = modelValidations;
		}

		private static readonly Dictionary<string, TypeAccessor> _typeAccessors = new Dictionary<string, TypeAccessor>();
		private TypeAccessor GetTypeAccessor(Type key)
		{
			if (_typeAccessors.ContainsKey(key.FullName)) return _typeAccessors[key.FullName];

			_typeAccessors[key.FullName] = TypeAccessor.Create(key);

			return _typeAccessors[key.FullName];
		}

		private Type GetModelType(Type type)
		{
			var args = type.GetType().GetGenericArguments();
			if (args.Length == 1)
			{
				args = args[0].GetGenericArguments();
				if (args.Length == 1)
				{
					return args[0];
				}
			}

			return null;
		}

		public INodeVisitor Validate(ValidationContext context)
		{
			return new EnterLeaveListener(cfg =>
			{
				cfg.Match<Argument>(arg =>
				{
					var argDef = context.TypeInfo.GetArgument();
					if (argDef == null) return;

					var type = argDef.ResolvedType;
					if (type.IsInputType())
					{
						var modelType = GetModelType(type.GetType());

						if (modelType == null) return;

						var modelValidations = _modelValidations.Where(x => x.CanHandle(modelType)).ToList();

						if (modelValidations.Count > 0)
						{
							var members = GetTypeAccessor(modelType).GetMembers().ToList();
							var o = arg.Value.Value;

							if (typeof(IEnumerable).IsAssignableFrom(o.GetType()))
							{
								foreach (IDictionary item in (IEnumerable)o)
								{
									foreach (string key in item.Keys)
									{
										var member = members.SingleOrDefault(x => x.Name.ToLower() == key.ToLower());
										if (member != null)
										{
											modelValidations.ForEach(modelValidation =>
											{
												string errorCode;
												string message;
												if (!modelValidation.TryAssertMemberValueIsValid(member, item[key], out errorCode, out message))
												{
													context.ReportError(new ValidationError(context.OriginalQuery, errorCode, message, arg));
												}
											});
										}
									}
								}
							}
						}
					}
				});
			});
		}

		private static string GetValue(Inputs input, string argumentName, string fieldTypeName)
		{
			if (input.ContainsKey(argumentName))
			{
				var model = (Dictionary<string, object>)input[argumentName];
				if (model != null && model.ContainsKey(fieldTypeName))
				{
					return model[fieldTypeName]?.ToString();
				}
			}

			return null;
		}
	}
}
