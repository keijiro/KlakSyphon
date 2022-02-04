KlakSyphon
==========

![screenshot](https://i.imgur.com/eputm6Am.jpg)

**KlakSyphon** is a [Syphon] plugin for Unity that allows sharing frames
between applications with minimum CPU/GPU cost.

The most important feature of KlakSyphon is that it supports [Metal]. You can
use Metal on Unity to utilize the GPU features and, at the same time, use Unity
in combinations with other Syphon-enabled OpenGL applications, like [VDMX] or
[MadMapper].

[Syphon]: http://syphon.v002.info
[Metal]: https://developer.apple.com/metal/
[VDMX]: http://vidvox.net
[MadMapper]: https://madmapper.com

System Requirements
-------------------

- Unity 2021.2
- Metal graphics API
- MacOS 12.1 Monterey

KlakSyphon only supports Metal; It doesn't support the OpenGL (GL Core) mode.

How to install
--------------

This package uses the [scoped registry] feature to resolve package dependencies.
Please add the following sections to the manifest file (Packages/manifest.json).

[scoped registry]: https://docs.unity3d.com/Manual/upm-scoped.html

To the `scopedRegistries` section:

```
{
  "name": "Keijiro",
  "url": "https://registry.npmjs.com",
  "scopes": [ "jp.keijiro" ]
}
```

To the `dependencies` section:

```
"jp.keijiro.klak.syphon": "0.0.4"
```

After changes, the manifest file should look like below:

```
{
  "scopedRegistries": [
    {
      "name": "Keijiro",
      "url": "https://registry.npmjs.com",
      "scopes": [ "jp.keijiro" ]
    }
  ],
  "dependencies": {
    "jp.keijiro.klak.syphon": "0.0.4",
...
```
