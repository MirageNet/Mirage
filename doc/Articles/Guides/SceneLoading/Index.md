# Loading scenes in Mirage

Mirage has [NetworkSceneManager](./NetworkSceneManager.md) to help load scenes and keep them in sync between server and client.

NetworkSceneManager also has virtual methods so you can create a new class and inherit from it in order to customize some of the logic.

Sometimes a game require more unique logic to load scenes, In that case you can control it manually following [This Guide](./Manual.md)