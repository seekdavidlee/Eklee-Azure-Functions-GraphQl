[Main page](../README.md)

# Introduction

If you are interested in hooking into the lifecycle of mutations, you can leverage two interfaces.

* IMutationPreAction
* IMutationPostAction

As the names denote, IMutationPreAction allows you to hook onto event when a mutation for an entity is about to happen. IMutationPostAction allows you hook onto event when a mutation on an entity has completed successfully. There is a enum called MutationActions that will tell you what type of mutation this is, i.e. Create, Update, Delete etc.

# Usage

You can implement your mutation action based on one of the interface and use AutoFac to hook it up.

```
builder.RegisterType<MyPostAction>().As<IMutationPostAction>().SingleInstance();
builder.RegisterType<MyPreAction>().As<IMutationPreAction>().SingleInstance();
```

## ExecutionOrder

All System Mutation Actions run with ExecutionOrder of 0. Some examples include the ConnectionEdgeHandler which is used to control and maintain Connection types of Models. You may wish to have a higher or lower ExecutionOrder value depending on your senario.

## What can I do?

If you have a requirement to persist the same entity in other forms of data store, then Mutation Actions is a perfect way for you to orchestrate that. However, be aware that any exceptions in the IMutationPreAction will cause the mutation to NOT happen. Thus, it is important for you to consider error handling and retry logic, especially in the context of the Cloud.

In the same way, if the mutation itself has an issue, the IMutationPostAction will NOT execute.

Refer to Azure's documentation for more information. For example, consider dropping the entity as a message into a queue for best performance whether it is post or pre actions.

Mutation Actions may also be a perfect way to notify of certain events. For example, you may consider pushing out an event via event hub for the mutation.