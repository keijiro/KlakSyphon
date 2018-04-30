KlakSyphon
==========

![screenshot](https://i.imgur.com/eputm6Am.jpg)
![screenshot](https://i.imgur.com/Y8Nc5YCm.jpg)
![screenshot](https://i.imgur.com/e4fl5lKm.jpg)

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

- Unity 2018.1
- Metal graphics API
- MacOS 10.12 Sierra

KlakSyphon only supports Metal; It doesn't support the OpenGL (GL Core) mode.

MacOS 10.11 El Capitan is also supported, but some Mac models (iMac/MacBook
with NVIDIA GPU) have problems with required functionalities on it. 10.12
Sierra is safer choice to support wider range of devices.

Installation
------------

Download and import one of the .unitypackage files from [Releases] page.

[Releases]: https://github.com/keijiro/KlakSyphon/releases

License
-------

- The Syphon framework is provided under the Syphon Framework License.
- The other part of the plugin is provided under the KlakSyphone License (MIT).

See the [LICENSE] file for further details.

[LICENSE]: LICENSE
