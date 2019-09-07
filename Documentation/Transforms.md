[Main page](../README.md)

# Transformation introduction
Model values can be transformed by your own custom implementation of IModelTransformer or one of the ones we created.

## System Transformers
System transformers will help us transform model with commonly used values. Enable it with the following:

```
builder.UseSystemModelTransformers();
```

See below for a list of System transformers.

### AutoId
AutoId will generated a GUID for a field such as a key field. Use the following code to enable AutoId on your field.

```
[AutoId]
[Key]
[Description("Id")]
public string Id { get; set; }
```

Use a placeholder value @ in place of a replacement value.

### AutoDateTime.
AutoDateTime will generated a current UTC date for your DateTime field. Use the following code to enable AutoDate on your field.

```
[AutoDateTime(AutoDateTimeTypes.UtcToday)]
[ModelField(false)]
[Description("FieldDateTimeToday")]
public DateTime FieldDateTimeToday { get; set; }
```

## ValueFromRequestContextGenerator
ValueFromRequestContextGenerator will pull some values from the request context to populate. Use the following code to 

```
builder.UseValueFromRequestContextGenerator();
```

In order to leverage ValueFromRequestContextGenerator, you will need to implement IRequestContextValueExtractor interface. An example of the interface implementation is below.

```
public class TrustFrameworkPolicyRequestContextValueExtractor : IRequestContextValueExtractor
{
	public Task<object> GetValueAsync(IGraphRequestContext graphRequestContext, Member member)
	{
		if (graphRequestContext.HttpRequest.Security != null)
		{
			var tfpClaim = graphRequestContext.HttpRequest.Security.ClaimsPrincipal.FindFirst(x => x.Type == "tfp");
			if (tfpClaim != null)
			{
				return Task.FromResult((object) tfpClaim.Value);
			}
		}

		throw new InvalidOperationException("Unable to get Trust Framework Policy (tfp) claim.");
	}
}
```

You can enable your custom implementation on the field.

```
[RequestContextValue(typeof(TrustFrameworkPolicyRequestContextValueExtractor))]
[ModelField(false)]
[Description("FieldFromTrustFrameworkPolicy")]
public string FieldFromTrustFrameworkPolicy { get; set; }
```