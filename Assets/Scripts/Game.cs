using UnityEngine;

public class Game : MonoBehaviour
{

	[SerializeField]
	Vector2Int boardSize = new Vector2Int(11, 11);

	[SerializeField]
	GameBoard board = default;

	[SerializeField]
	GameTileContentFactory tileContentFactory = default;

	[SerializeField]
	WarFactory warFactory = default;

	[SerializeField, Range(0, 100)]
	int startingPlayerHealth = 10;

	[SerializeField, Range(1f, 10f)]
	float playSpeed = 1f;

	int playerHealth;

	const float pausedTimeScale = 0f;

	static Game instance;

	GameBehaviourCollection enemies = new GameBehaviourCollection();
	GameBehaviourCollection nonEnemies = new GameBehaviourCollection();

	TowerType selectedTowerType;

	Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

	[SerializeField]
	GameScenario scenario = default;

	GameScenario.State activeScenario;

	private void OnEnable()
	{
		instance = this;
	}

	void Awake()
	{
		playerHealth = startingPlayerHealth;
		board.Initialize(boardSize, tileContentFactory);
		board.ShowGrid = true;
		activeScenario = scenario.Begin();
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

		if (Input.GetKeyDown(KeyCode.Space))
		{
			Time.timeScale = Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
		}
		else if (Time.timeScale > pausedTimeScale)
		{
			Time.timeScale = playSpeed;
		}

		if (Input.GetKeyDown(KeyCode.N))
		{
			BeginNewGame();
		}

		if (playerHealth <= 0 && startingPlayerHealth > 0)
		{
			Debug.Log("Defeat!");
			BeginNewGame();
		}

		if (!activeScenario.Progress() && enemies.IsEmpty)
		{
			Debug.Log("Victory!");
			BeginNewGame();
			activeScenario.Progress();
		}

		enemies.GameUpdate();
		Physics.SyncTransforms();	
		board.GameUpdate();
		nonEnemies.GameUpdate();
	}

	void BeginNewGame()
	{
		playerHealth = startingPlayerHealth;
		enemies.Clear();
		nonEnemies.Clear();
		board.Clear();
		activeScenario = scenario.Begin();
	}

	public static void EnemyReachedDestination()
	{
		instance.playerHealth -= 1;
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

	public static void SpawnEnemy(EnemyFactory factory, EnemyType type)
	{
		GameTile spawnPoint = instance.board.GetSpawnPoint(Random.Range(0, instance.board.SpawnPointCount));
		Enemy enemy = factory.Get(type);
		enemy.SpawnOn(spawnPoint);
		instance.enemies.Add(enemy);
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
}