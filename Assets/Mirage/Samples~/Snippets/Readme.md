This folder has code that is used by the codes.

It is valid mirage code and can be used as examples, but they may be incomplete examples so might be missing context around them.

## How the embedding works
The docs run [EmbedCodeInMarkdown](https://github.com/James-Frowen/EmbedCodeInMarkdown) before building, which will embed the code.


### In code

use `CodeEmbed-Start` and `CodeEmbed-End` in code too allow a block to be found

```cs
// CodeEmbed-Start: block-name
[ClientRpc]
public void MyFunction(int number) 
{
    // do stuff
}
// CodeEmbed-End: block-name
```

### In markdown

To embed a block of code from "Assets/Mirage/Samples~/" you need to add a special comment in the following format:

```
{{{ Path:'path/to/code' Name:'block-name' }}}
```

example: 
```
{{{ Path:'Snippets/SendNetworkMessage.cs' Name:'send-score' }}}
```
