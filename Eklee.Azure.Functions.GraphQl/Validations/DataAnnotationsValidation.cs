using FastMember;
using GraphQL;
using GraphQL.Language.AST;
using GraphQL.Types;
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

		private Type GetModelType(IGraphType type)
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

		private Type FilterSearchTypes(Type type)
		{
			if (type == null) return null;

			if (type.FullName.StartsWith("Eklee.Azure.Functions.GraphQl.Filters"))
			{
				return null;
			}

			return type;
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
						var modelType = FilterSearchTypes(GetModelType(type));

						if (modelType == null) return;

						var modelValidations = _modelValidations.Where(x => x.CanHandle(modelType)).ToList();

						if (modelValidations.Count > 0)
						{
							var ta = GetTypeAccessor(modelType);
							var members = ta.GetMembers().ToList();
							var o = arg.Value.Value;

							if (typeof(IEnumerable).IsAssignableFrom(o.GetType()))
							{
								if (o is IDictionary)
								{
									Validate((Dictionary<string, object>)o,
										members, modelValidations, context, arg);
								}

								if (o is IList)
								{
									foreach (var oItem in (IList)o)
									{
										Validate((Dictionary<string, object>)oItem,
											members, modelValidations, context, arg);
									}
								}
							}
						}
					}
				});
			});
		}

		private static void Validate(Dictionary<string, object> items,
			List<Member> members,
			List<IModelValidation> modelValidations,
			ValidationContext context,
			Argument arg)
		{
			foreach (var item in items)
			{
				var member = members.SingleOrDefault(x => x.Name.ToLower() == item.Key.ToLower());
				if (member != null)
				{
					modelValidations.ForEach(modelValidation =>
					{
						string errorCode;
						string message;
						if (!modelValidation.TryAssertMemberValueIsValid(member, items[item.Key], out errorCode, out message))
						{
							context.ReportError(new ValidationError(context.OriginalQuery, errorCode, message, arg));
						}
					});
				}
			}
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
