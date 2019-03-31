[Main page](../README.md)

# Validation introduction
Model validation can be leverage by performing 2 simple steps. Create your Model and add a supported attribute from System.ComponentModel.DataAnnotations. Next, add the following in your AutoFac Module.

```
builder.UseDataAnnotationsValidation();
```

## Currently supported attribute

* StringLengthAttribute

## Custom Model Validation

You can also validate your Model via a custom validator. You will need to implement the IModelValidation interface. An example below.

```
	public class MyValidation : IModelValidation
	{
		public bool CanHandle(Type type)
		{
			return type == typeof(Model4);
		}

		public bool TryAssertMemberValueIsValid(Member member, object value, out string errorCode, out string message)
		{
			if (member.Name == "DateField")
			{
				DateTime result;
				if (DateTime.TryParse(value.ToString(), out result))
				{
					if (result == DateTime.MinValue)
					{
						errorCode = "DateTimeError";
						message = "DateTime cannot be Min Value.";
						return false;
					}
				}
				else
				{
					errorCode = "DateTimeError";
					message = "DateTime is invalid.";
					return false;
				}
			}

			errorCode = null;
			message = null;
			return true;
		}
	}
```

Lastly, register your custom validator in your AutoFac Module.

```
builder.RegisterType<MyValidation>().As<IModelValidation>();
```