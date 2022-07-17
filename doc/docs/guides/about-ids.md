---
sidebar_position: 3
---
# About IDs

## NetId

Mirage uses `uint` for NetId. Every NetworkIdentity is assigned a NetId when it is being spawned. Mirage uses the ID when 
passing messages between client and server to tell which object is the recipient of the message.

## Prefab Hash

The prefab hash is used to uniquely identify each prefab so that they can be spawned over the network. 
Mirage create the prefab hash by taking a 32 bit hash of the Asset path. 
The path is found using [AssetDatabase.GetAssetPath](https://docs.unity3d.com/ScriptReference/AssetDatabase.GetAssetPath.html)

## Scene ID

Mirage uses `ulong` for Scene IDss. Every game object with a NetworkIdentity in the scene (hierarchy) is assigned a scene 
ID in OnPostProcessScene. Mirage needs that to distinguish scene objects from each other, because Unity has no unique 
ID for different game objects in the scene.