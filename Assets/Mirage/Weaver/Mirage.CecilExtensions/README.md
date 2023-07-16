# Mirage.Cecil

Extensions to help use Mono.Cecil inside Unity


## How to use

- Create class that inherits from `ILPostProcessor`
    - Use `ILPPHelper` for default Implementation
- Create class that inherits from `WeaverBase`
- Return your `Weaver` class from `BaseILPostProcessor.CreateWeaver`
- Process your assembly in `WeaverBase.Process(AssemblyDefinition)`
- Return if it was success or not