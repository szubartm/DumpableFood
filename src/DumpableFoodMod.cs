using Mafi;
using Mafi.Collections;
using Mafi.Core.Mods;

namespace DumpableFood;

public sealed class DumpableFoodMod : DataOnlyMod
{
    public DumpableFoodMod(ModManifest manifest) : base(manifest)
    {
        Log.Info("DumpableFood: constructed");
    }

    public override void RegisterPrototypes(ProtoRegistrator registrator)
    {
        Log.Info("DumpableFood: registering prototypes");
        DumpableFood.Configure(registrator);
    }

    public override void MigrateJsonConfig(VersionSlim savedVersion, Dict<string, object> savedValues)
    {
    }
}