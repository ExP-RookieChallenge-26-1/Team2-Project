using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public class BallLowTerrainPassThroughTests
{
	[Test]
	public void ShouldIgnoreLowTerrainCollisionReturnsTrueForUpwardUndersideHitWhenConnectedTerrainIsBelowPassThroughY()
	{
		Bounds hitTileBounds = new Bounds(new Vector3(0f, -5.5f, 0f), Vector3.one);
		Bounds connectedTerrainBounds = new Bounds(new Vector3(0f, -5.5f, 0f), new Vector3(2f, 1f, 1f));

		Assert.That(
			ShouldIgnoreLowTerrainCollision(
				hitTileBounds,
				connectedTerrainBounds,
				new Vector2(0f, -6.1f),
				0.2f,
				Vector2.up * 4f,
				true,
				-3f),
			Is.True);
	}

	[Test]
	public void ShouldIgnoreLowTerrainCollisionReturnsTrueWhenConnectedTerrainBottomIsBelowPassThroughY()
	{
		Bounds hitTileBounds = new Bounds(new Vector3(0f, -3.5f, 0f), Vector3.one);
		Bounds connectedTerrainBounds = new Bounds(new Vector3(0f, -3f, 0f), new Vector3(1f, 4f, 1f));

		Assert.That(
			ShouldIgnoreLowTerrainCollision(
				hitTileBounds,
				connectedTerrainBounds,
				new Vector2(0f, -4.1f),
				0.2f,
				Vector2.up * 4f,
				true,
				-3f),
			Is.True);
	}

	[Test]
	public void ShouldIgnoreLowTerrainCollisionReturnsFalseWhenConnectedTerrainBottomIsAbovePassThroughY()
	{
		Bounds hitTileBounds = new Bounds(new Vector3(0f, -2.2f, 0f), Vector3.one);
		Bounds connectedTerrainBounds = new Bounds(new Vector3(0f, -2.2f, 0f), Vector3.one);

		Assert.That(
			ShouldIgnoreLowTerrainCollision(
				hitTileBounds,
				connectedTerrainBounds,
				new Vector2(0f, -2.9f),
				0.2f,
				Vector2.up * 4f,
				true,
				-3f),
			Is.False);
	}

	[Test]
	public void ShouldIgnoreLowTerrainCollisionReturnsFalseForDownwardTopHit()
	{
		Bounds hitTileBounds = new Bounds(new Vector3(0f, -5.5f, 0f), Vector3.one);
		Bounds connectedTerrainBounds = new Bounds(new Vector3(0f, -5.5f, 0f), Vector3.one);

		Assert.That(
			ShouldIgnoreLowTerrainCollision(
				hitTileBounds,
				connectedTerrainBounds,
				new Vector2(0f, -4.9f),
				0.2f,
				Vector2.down * 4f,
				true,
				-3f),
			Is.False);
	}

	[Test]
	public void ShouldIgnoreLowTerrainCollisionReturnsFalseWhenFeatureIsDisabled()
	{
		Bounds hitTileBounds = new Bounds(new Vector3(0f, -5.5f, 0f), Vector3.one);
		Bounds connectedTerrainBounds = new Bounds(new Vector3(0f, -5.5f, 0f), Vector3.one);

		Assert.That(
			ShouldIgnoreLowTerrainCollision(
				hitTileBounds,
				connectedTerrainBounds,
				new Vector2(0f, -6.1f),
				0.2f,
				Vector2.up * 4f,
				false,
				-3f),
			Is.False);
	}

	private static bool ShouldIgnoreLowTerrainCollision(
		Bounds hitTileBounds,
		Bounds connectedTerrainBounds,
		Vector2 pos,
		float radius,
		Vector2 velocity,
		bool isEnabled,
		float passThroughMaxY)
	{
		MethodInfo method = typeof(BallCollision).GetMethod(
			"ShouldIgnoreLowTerrainCollision",
			BindingFlags.Public | BindingFlags.Static);
		Assert.That(method, Is.Not.Null, "BallCollision should expose the low-terrain pass-through predicate for deterministic tests.");

		return (bool)method.Invoke(
			null,
			new object[] { hitTileBounds, connectedTerrainBounds, pos, radius, velocity, isEnabled, passThroughMaxY });
	}
}
