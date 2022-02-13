# Rimefeller Tanker

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/E1E120FY7)

Feel free to use in your own projects. If you do, please consider crediting me and including my Ko-Fi link: https://ko-fi.com/sokyran

There's built-in native Multiplayer support.

To set the textures for gizmos, look into included Textures directory - it'll contain 2 text files. You need to use graphical files supported by RimWorld with the same name - but obviously different extension thatn txt.

To use this component in your things, first, make sure that your thing also has `CompProperties_Pipe` component attached.
Second, add the component to your thing:

```xml
<li Class="RimefellerTanker.CompProperties_RimefellerTanker">
  <!--Default values-->
  <contents>Fuel</contents>
  <storageCap>10000</storageCap>
  <fillAmount>0.5</fillAmount>
  <drainAmount>0.5</drainAmount>
</li>
```

If you want your thing to display a bar displaying current fill level when selected, you must set ThingDef's drawerType to MapMeshAndRealTime like so:
```xml
<drawerType>MapMeshAndRealTime</drawerType>
```