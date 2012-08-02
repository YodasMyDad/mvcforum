The EFProvider Wrappers are taken from "Tracing and Caching Provider Wrappers for Entity Framework", on MSDN:

http://code.msdn.microsoft.com/EFProviderWrappers-c0b88f32

The caching provider classes are included here. There is a small change to the original code, in the 
module "DBConnectionWrapper" in the "EFProviderToolkit" project. At line 257 the wrapped connection is
now checked to see whether it is null before disposal.