---
id: SyncHookType
title: SyncHookType
---

# Enum SyncHookType




##### Syntax

```cs
public enum SyncHookType
```


### Fields

#### Automatic

Looks for hooks matching the signature, gives compile error if none or more than 1 is found


##### Declaration

```cs
Automatic = 0
```
#### MethodWith0Arg

Hook with signature void hookName()


##### Declaration

```cs
MethodWith0Arg = 1
```
#### MethodWith1Arg

Hook with signature void hookName(T newValue)


##### Declaration

```cs
MethodWith1Arg = 2
```
#### MethodWith2Arg

Hook with signature void hookName(T oldValue, T newValue)


##### Declaration

```cs
MethodWith2Arg = 3
```
#### EventWith0Arg

Hook with signature event Action hookName;


##### Declaration

```cs
EventWith0Arg = 4
```
#### EventWith1Arg

Hook with signature event Action{T} hookName;


##### Declaration

```cs
EventWith1Arg = 5
```
#### EventWith2Arg

Hook with signature event Action{T,T} hookName;


##### Declaration

```cs
EventWith2Arg = 6
```
