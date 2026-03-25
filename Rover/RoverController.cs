using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using static RoverInterpreter;

public class RoverController : MonoBehaviour
{
    // Interpreter
    CodeUploader uploader;

    [Header("Level manager")]
    public LevelManager levelManager;

    [Header("Rover Tiles")]
    public TileBase roverTileCurrent;
    public Vector3Int roverTileCurrentPos;
    public TileBase roverTileForward;
    public TileBase roverTileLeft;
    public TileBase roverTileRight;

    [Header("Stats")]
    public bool animatingCode;
    public bool canMove;
    public bool win;

    [Header("Level config")]
    public LevelConfig levelConfig;

    [Header("Debugging information")]
    public TextMeshProUGUI debugText;

    [Header("Tilemap")]
    public Tilemap levelTilemap;
    public TileBase emptyTile;

    [Header("Animation")]
    public AnimationCurve moveCurve;
    public float commandsPerSecond;
    public float commandTime;

    [Header("Console")]
    public TextMeshProUGUI consoleText;

    [Header("Win/Lose")]
    public LevelComplete levelComplete;
    public DebugString roverLeftPathMessage;
    public DebugString roverReachedGoalMessage;
    public DebugString roverDidNotReachGoalMessage;
    string FormatDebugString(DebugString message) => $"<color=#{message.color.ToHexString()}>" + message.message + "</color>\n";


    [System.Serializable]
    // Каждая из команд представляет из себя свой тип и сообщение в консоль
    public struct Command
    {
        public CommandType type;
        public DebugString debug;
    }

    // Command presets
    Command moveCommand  = new Command { type = CommandType.MOVE, debug = new DebugString { message = "", color = Color.white} };
    Command leftCommand  = new Command { type = CommandType.TURN_LEFT, debug = new DebugString { message = "", color = Color.white } };
    Command rightCommand = new Command { type = CommandType.TURN_RIGHT, debug = new DebugString { message = "", color = Color.white } };

    // Все возможные типы команд
    public enum CommandType
    {
        MOVE,       // Сделать шаг вперёд
        TURN_LEFT,  // Повернуть налево
        TURN_RIGHT, // Повернуть направо
        PRINT_DEBUG // Отправить сообщение в консоль
    }

    [Header("Command buffer")]
    public List<Command> commandBuffer = new List<Command>(); // Буфер команд

    public void MasterReset()
    {
        ResetBuffer();
        ResetRover();
    }

    void Start()
    {
        uploader = GetComponent<CodeUploader>();

        levelConfig = levelManager.levelConfig;
        
        animatingCode = false;

        ResetRover();
    }

    public void AddDebugString(string info)
    {
        debugText.text += $"{info}\n";
    }

    // КОМАНДНЫЕ МЕТОДЫ: методы, исполняющие команды из буфера
    #region
    // Starts the ExecuteCommands coroutine
    public void RunProgram()
    {
        ResetRover();
        StartCoroutine(ExecuteCommands(commandBuffer));
    }

    // Исполняет команды из буфера по-очереди
    public IEnumerator ExecuteCommands(List<Command> commands)
    {
        // Обновить переменные
        animatingCode = true;

        // Итерировать через буфер, исполняя каждую команду
        foreach (Command command in commands)
        {
            switch (command.type)
            {
                case CommandType.MOVE:
                    yield return StartCoroutine(MoveForward());
                    break;
                case CommandType.TURN_LEFT:
                    yield return StartCoroutine(TurnLeft());
                    break;
                case CommandType.TURN_RIGHT:
                    yield return StartCoroutine(TurnRight());
                    break;
                case CommandType.PRINT_DEBUG:
                    yield return StartCoroutine(PrintDebug(command));
                    break;
            }

            // Если в любой момент ровер съезжает с дороги, остановить процесс и оставить сообщение в консоли: игрок проиграл.
            if (roverTileCurrent == emptyTile){
                consoleText.text += FormatDebugString(roverLeftPathMessage);
                animatingCode = false;
                yield break;
            }
        }

        // Ровер исполнил все команды.
        animatingCode = false;

        // Определить, достигнута ли цель или нет.
        win = roverTileCurrent.name.Contains("Goal");
        if (win)
        {
            // Если игрок победил, показать экран Level Complete
            consoleText.text += FormatDebugString(roverReachedGoalMessage);
            levelComplete.LevelCompleteProcedure(uploader.lineCount);
        }
        else
        {
            // Если нет, отправить сообщение в консоль
            consoleText.text += FormatDebugString(roverDidNotReachGoalMessage);
        }

        // Остановить корутину
        yield return null;
    }
    #endregion
    
    // БУФЕРНЫЕ МЕТОДЫ: Методы для редактирования буфера
    #region
    // Очистить буфер
    public void ResetBuffer()
    {
        commandBuffer = new List<Command>();
    }

    // Добавлять различные команды в буфер
    public void AddMoveForwardCommand()
    {
        commandBuffer.Add(moveCommand);
    }
    
    public void AddTurnLeftCommand()
    {
        commandBuffer.Add(leftCommand);
    }
    
    public void AddTurnRightCommand()
    {
        commandBuffer.Add(rightCommand);
    }
    
    public void AddPrintDebugCommand(DebugString debugString) // Команда включает в себя сообщение DebugString
    {
        commandBuffer.Add(new Command { type = CommandType.PRINT_DEBUG, debug = debugString});
    }
    #endregion

    // РОВЕРНЫЕ МЕТОДЫ: Методы, анимирующие ровер.
    #region
    // Resets the rover transform and the debug textbox
    public void ResetRover()
    {
        transform.position = levelTilemap.CellToWorld(new Vector3Int(levelConfig.startPosition.x, levelConfig.startPosition.y, 0)) + Vector3.one / 2;
        transform.forward = new Vector3Int(levelConfig.startForward.x, 0, levelConfig.startForward.y);
        consoleText.text = "";
    }
    
    // Двигает ровер вперёд на один шаг
    public IEnumerator MoveForward()
    {
        Vector3 posRec = transform.position;

        commandTime = 0;

        while (commandTime <= 1)
        {
            transform.position = posRec + transform.forward * moveCurve.Evaluate(commandTime);

            commandTime += Time.deltaTime * commandsPerSecond;

            yield return new WaitForEndOfFrame();
        }

        transform.position = posRec + transform.forward;

        // Обновить ячейку ровера
        roverTileCurrentPos = levelTilemap.WorldToCell(transform.position);
        roverTileCurrent = levelTilemap.GetTile(roverTileCurrentPos);

        yield return null;
    }

    // Поварачивает ровер налево на 90 градусов
    public IEnumerator TurnLeft()
    {
        float yaw = transform.localRotation.eulerAngles.y;
        commandTime = 0;

        while (commandTime <= 1)
        {
            transform.localRotation = Quaternion.Euler(transform.rotation.x, yaw - 90 * moveCurve.Evaluate(commandTime), transform.rotation.z);

            commandTime += Time.deltaTime * commandsPerSecond;

            yield return new WaitForEndOfFrame();
        }

        transform.localRotation = Quaternion.Euler(transform.rotation.x, yaw - 90, transform.rotation.z);

        yield return null;
    }

    // Поварачивает ровер направо на 90 градусов
    public IEnumerator TurnRight()
    {
        float yaw = transform.localRotation.eulerAngles.y;
        commandTime = 0;

        while (commandTime <= 1)
        {
            transform.localRotation = Quaternion.Euler(transform.rotation.x, yaw + 90 * moveCurve.Evaluate(commandTime), transform.rotation.z);

            commandTime += Time.deltaTime * commandsPerSecond;

            yield return new WaitForEndOfFrame();
        }

        transform.localRotation = Quaternion.Euler(transform.rotation.x, yaw + 90, transform.rotation.z);

        yield return null;
    }

    // Отправляет сообщение в консоль
    public IEnumerator PrintDebug(Command debugCommand)
    {
        consoleText.text += FormatDebugString(debugCommand.debug);
        yield return new WaitForSeconds(1 / commandsPerSecond);
    }
    #endregion
}