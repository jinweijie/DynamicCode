# DynamicCode
Dynamically compile and run C# code in .Net Core.

# Limitation
This application currently works on .Net Core 2.2, but since AssemblyLoadContext in .Net Core version 2.2 and below does not support Unload assembly, the dynamic created assembly will be removed from runtime. 

Please use this code with caution.

In .Net Core 3.0, the Unload api will be added.
