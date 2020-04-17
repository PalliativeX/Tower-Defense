﻿using UnityEngine;

public class Game : MonoBehaviour
{

	[SerializeField]
	Vector2Int boardSize = new Vector2Int(11, 11);

	[SerializeField]
	GameBoard board = default;

	[SerializeField]
	GameTileContentFactory tileContentFactory = default;

	[SerializeField]
	EnemyFactory enemyFactory = default;

	[SerializeField]
	WarFactory warFactory = default;

	static Game instance;

	[SerializeField, Range(0.1f, 10f)]
	float spawnSpeed = 1f;

	float spawnProgress;

	GameBehaviourCollection enemies = new GameBehaviourCollection();
	GameBehaviourCollection nonEnemies = new GameBehaviourCollection();

	TowerType selectedTowerType;

	Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

	private void OnEnable()
	{
		instance = this;
	}

	void Awake()
	{
		board.Initialize(boardSize, tileContentFactory);
		board.ShowGrid = true;
	}

	void OnValidate()
	{
		if (boardSize.x < 2)
		{
			boardSize.x = 2;
		}
		if (boardSize.y < 2)
		{
			boardSize.y = 2;
		}
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			HandleTouch();
		}
		else if (Input.GetMouseButtonDown(1))
		{
			HandleAlternativeTouch();
		}

		if (Input.GetKeyDown(KeyCode.V))
		{
			board.ShowPaths = !board.ShowPaths;
		}
		if (Input.GetKeyDown(KeyCode.G))
		{
			board.ShowGrid = !board.ShowGrid;
		}

		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			selectedTowerType = TowerType.Laser;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			selectedTowerType = TowerType.Mortar;
		}

		spawnProgress += spawnSpeed * Time.deltaTime;
		while (spawnProgress >= 1f)
		{
			spawnProgress -= 1f;
			SpawnEnemy();
		}
		enemies.GameUpdate();
		Physics.SyncTransforms();
		board.GameUpdate();
		nonEnemies.GameUpdate();
	}

	public static Shell SpawnShell()
	{
		Shell shell = instance.warFactory.Shell;
		instance.nonEnemies.Add(shell);
		return shell;
	}

	public static Explosion SpawnExplosion()
	{
		Explosion explosion = instance.warFactory.Explosion;
		instance.nonEnemies.Add(explosion);
		return explosion;
	}

	void HandleAlternativeTouch()
	{
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null)
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				board.ToggleDestination(tile);
			}
			else
			{
				board.ToggleSpawnPoint(tile);
			}
		}
	}

	void HandleTouch()
	{
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null)
		{
			if (Input.GetKey(KeyCode.LeftShift))
			{
				board.ToggleTower(tile, selectedTowerType);
			}
			else
			{
				board.ToggleWall(tile);
			}
		}
	}

	void SpawnEnemy()
	{
		GameTile spawnPoint = board.GetSpawnPoint(Random.Range(0, board.SpawnPointCount));
		Enemy enemy = enemyFactory.Get();
		enemy.SpawnOn(spawnPoint);
		enemies.Add(enemy);
	}
}