## [Back](../README.md)

# Introduction

Mutation is the process by which data is ingested into the system. The system can automatically generate mutation schema based on your model.

The following is an example of declaring a class called MyMutation. You would use this in the context of Schema declaration.

```
	public class MyMutation : ObjectGraphType
	{
	}
```

## Repositories

With mutation also comes the concept of where the entity type data is persisted to. This is why we have created different repositories.

Currently, the following repository types are supported.

- HttpRepository
- DocumentDbRepository (Azure CosmosDb, SQL Core)
- InMemoryRepository

** More documentation is coming. **