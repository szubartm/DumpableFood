using System;
using Mafi;
using Mafi.Base;
using Mafi.Collections.ImmutableCollections;
using Mafi.Core.Buildings.Farms;
using Mafi.Core.Mods;
using Mafi.Core.Products;
using Mafi.Core.Prototypes;

namespace DumpableFood;

internal static class DumpableFood {

	private readonly struct FoodTerrainSpec(
		ProductProto.ID productId,
		string materialKey,
		string nameInEditor,
		Proto.ID baseTerrainId,
		string albedoHeightPath,
		string normalSaoPath,
		ColorRgba color,
		DetailLayerSpecProto.ID? detailId = null,
		float detailDensity = 0f
	) {
		public ProductProto.ID ProductId { get; } = productId;
		public string MaterialKey { get; } = materialKey;
		public string NameInEditor { get; } = nameInEditor;
		public Proto.ID BaseTerrainId { get; } = baseTerrainId;
		public string AlbedoHeightPath { get; } = albedoHeightPath;
		public string NormalSaoPath { get; } = normalSaoPath;
		public ColorRgba Color { get; } = color;
		public DetailLayerSpecProto.ID? DetailId { get; } = detailId;
		public float DetailDensity { get; } = detailDensity;
	}

	private static readonly FoodTerrainSpec[] Specs = [
		new(
			Ids.Products.Potato, "Potato", "Potato",
			Ids.TerrainMaterials.DirtBare,
			"Assets/Base/Terrain/Textures/Bauxite-disrupted-2048-albedo-height.png",
			"Assets/Base/Terrain/Textures/Bauxite-disrupted-2048-normal-s-ao.png",
			new ColorRgba(176, 134, 78)
		),
		new(
			Ids.Products.Corn, "Corn", "Corn",
			Ids.TerrainMaterials.SandDisrupted,
			"Assets/Base/Terrain/Textures/Sand-disrupted-2048-albedo-height.png",
			"Assets/Base/Terrain/Textures/Sand-disrupted-2048-normal-s-ao.png",
			new ColorRgba(255, 224, 72),
			Ids.TerrainDetails.GrassLush, 0.2f
		),
		new(
			Ids.Products.Wheat, "Wheat", "Wheat",
			Ids.TerrainMaterials.SandDisrupted,
			"Assets/Base/Terrain/Textures/Sand-disrupted-2048-albedo-height.png",
			"Assets/Base/Terrain/Textures/Sand-disrupted-2048-normal-s-ao.png",
			new ColorRgba(222, 184, 92)
		),
		new(
			Ids.Products.Soybean, "Soybean", "Soybean",
			Ids.TerrainMaterials.SandDisrupted,
			"Assets/Base/Terrain/Textures/Sand-disrupted-2048-albedo-height.png",
			"Assets/Base/Terrain/Textures/Sand-disrupted-2048-normal-s-ao.png",
			new ColorRgba(248, 212, 92)
		),
		new(
			Ids.Products.SugarCane, "SugarCane", "Sugar cane",
			Ids.TerrainMaterials.Compost,
			"Assets/Base/Terrain/Textures/ForestFloor-2048-albedo-height.png",
			"Assets/Base/Terrain/Textures/ForestFloor-2048-normal-s-ao.png",
			new ColorRgba(148, 172, 84)
		),
		new(
			Ids.Products.Canola, "Canola", "Canola",
			Ids.TerrainMaterials.DirtBare,
			"Assets/Base/Terrain/Textures/Coal-disrupted-2048-albedo-height.png",
			"Assets/Base/Terrain/Textures/Coal-disrupted-2048-normal-s-ao.png",
			new ColorRgba(68, 64, 58)
		),
		new(
			Ids.Products.Poppy, "Poppy", "Poppy",
			Ids.TerrainMaterials.DirtBare,
			"Assets/Base/Terrain/Textures/Coal-disrupted-2048-albedo-height.png",
			"Assets/Base/Terrain/Textures/Coal-disrupted-2048-normal-s-ao.png",
			new ColorRgba(82, 84, 92)
		),
		new(
			Ids.Products.Sugar, "Sugar", "Sugar",
			Ids.TerrainMaterials.LimestoneDisrupted,
			"Assets/Base/Terrain/Textures/Limestone-2048-albedo-height.png",
			"Assets/Base/Terrain/Textures/Limestone-2048-normal-s-ao.png",
			new ColorRgba(255, 255, 255)
		),
		new(
			Ids.Products.AnimalFeed, "AnimalFeed", "Animal feed",
			Ids.TerrainMaterials.SandDisrupted,
			"Assets/Base/Terrain/Textures/Sand-disrupted-2048-albedo-height.png",
			"Assets/Base/Terrain/Textures/Sand-disrupted-2048-normal-s-ao.png",
			new ColorRgba(255, 232, 130)
		),
	];

	public static void Configure(ProtoRegistrator registrator) {
		try {
			foreach (var spec in Specs) {
				RegisterDumpableProduct(registrator, spec);
			}
			Log.Info($"DumpableFood: Configured terrain materials for {Specs.Length} loose food products.");
		}
		catch (Exception ex) {
			Log.Exception(ex);
		}
	}

	private static void RegisterDumpableProduct(ProtoRegistrator registrator, FoodTerrainSpec spec) {
		var product = registrator.PrototypesDb.GetOrThrow<LooseProductProto>(spec.ProductId);
		var baseTerrain = registrator.PrototypesDb.GetOrThrow<TerrainMaterialProto>(spec.BaseTerrainId);

		var terrainId = new Proto.ID($"DumpableFood.TerrainMaterial_{spec.MaterialKey}{TerrainMaterialProto.SUFFIX}");

		var details = spec.DetailId.HasValue
			? ImmutableArray.Create(new DetailLayerSpec(
				registrator.PrototypesDb.GetOrThrow<DetailLayerSpecProto>(spec.DetailId.Value),
				spec.DetailDensity
			))
			: ImmutableArray<DetailLayerSpec>.Empty;

		var gfx = baseTerrain.Graphics.WithReplaced(
			albedoHeightTexturePath: spec.AlbedoHeightPath,
			normalSaoTexturePath: spec.NormalSaoPath,
			detailLayers: details
		);
		gfx.Color = (ColorUniversal) spec.Color.SetA(byte.MaxValue);
		gfx.ParticleColor = (ColorUniversal) spec.Color.SetA(byte.MaxValue);

		var terrain = new TerrainMaterialProto(
			id: terrainId,
			nameInEditor: spec.NameInEditor,
			minedProduct: product,
			minedQuantityMult: baseTerrain.MinedQuantityMult,
			minCollapseHeightDiff: baseTerrain.MinCollapseHeightDiff,
			maxCollapseHeightDiff: baseTerrain.MaxCollapseHeightDiff,
			graphics: gfx,
			ignoreInEditor: true,
			useSoftCollapseSound: true
		);

		registrator.PrototypesDb.Add(terrain);
		product.AddOrReplaceParam(new LooseProductParam(terrainId));
	}
}