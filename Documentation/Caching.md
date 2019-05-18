[Main page](../README.md)

# Introduction

To setup caching, in your Module setup, use the extension method UseDistributedCache. Note that MemoryDistributedCache is just an example. In a production senario, you may choose something like Azure Redis.

```
builder.UseDistributedCache<MemoryDistributedCache>();
```