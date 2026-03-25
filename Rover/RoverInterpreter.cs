using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using PimDeWitte.UnityMainThreadDispatcher;
using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class RoverInterpreter : MonoBehaviour
{
    [Serializable]
    public struct DebugString
    {
        public string message;
        public Color color;
    }

    ScriptEngine engine;
    ScriptScope scope;
    public int maxCommands;
    public float timeoutSeconds;
    bool canMoveForward, canMoveLeft, canMoveRight;

    [Header("Debugger")]
    public Color debugColor;
    public Color errorColor;

    // Ghost rover state
    Vector2Int ghostRoverPos;
    Vector2Int ghostRoverForward;

    // Helper methods to rotate forward vector in 90-degree angles.
    Vector2Int RotateLeft(Vector2Int vector) => new Vector2Int(-vector.y, vector.x);
    Vector2Int RotateRight(Vector2Int vector) => new Vector2Int(vector.y, -vector.x);

    RoverController controller;

    void Start()
    {
        controller = GetComponent<RoverController>();
        engine = Python.CreateEngine();
        scope = engine.CreateScope();
        scope.SetVariable("rover", this);
    }

    public void ExecuteCode(string playerCode)
    {
        // Обновить виртуальный ровер
        ghostRoverPos = new Vector2Int(controller.levelConfig.startPosition.x, controller.levelConfig.startPosition.y);
        ghostRoverForward = new Vector2Int(controller.levelConfig.startForward.x, controller.levelConfig.startForward.y);

        controller.MasterReset();
        controller.win = false;

        UpdateTiles();

        // Запустить поток интерпретора
        Thread interpreterThread = new Thread(() => RunPython(playerCode));
        interpreterThread.Start();

        // Запустить таймер для потока
        StartCoroutine(MonitorInterpreter(interpreterThread));
    }

    private void RunPython(string code)
    {
        try
        {
            engine.Execute(code, scope);

            // Определить, если ровер достиг цели или нет на главном потоке
            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                var tile = controller.levelTilemap.GetTile(new Vector3Int(ghostRoverPos.x, ghostRoverPos.y, 0));
                controller.win = tile != null && tile.name.Contains("Goal");
            });
        }
        catch (ThreadAbortException) { /* Handle timeout */ }
        catch (Exception e)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                var traceback = engine.GetService<ExceptionOperations>();
                RaiseError("Python Error: " + traceback.FormatException(e));
            });
        }

        UnityMainThreadDispatcher.Instance().Enqueue(() => controller.RunProgram());
    }

    private IEnumerator MonitorInterpreter(Thread thread)
    {
	// Запускаем таймер для потока.        
	float timer = 0;
        while (thread.IsAlive && timer < timeoutSeconds)
        {
	    // Пока интерпретатор работает над кодом таймер тоже будет работать
            timer += Time.unscaledDeltaTime;
            yield return null;
        }
	
	// Если интерпретатаор всё ещё работает, то код игрока занял слишком много времени из-за бесконечного цикла. Останавливаем поток вручную и высвечиваем ошибку.
        if (thread.IsAlive)
        {
            thread.Abort();
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
		// Важно: заменяем все команды уже в буфере одной командой отправки сообщения в консоль
	        controller.ResetBuffer();
                RaiseError("<i>Код занял слишком много времени. Возможно, в нем есть бесконечный цикл.</i>");
                controller.RunProgram();
            });
        }
    }

    // Обновляет ячейки
    public void UpdateTiles()
    {
        // Спереди
        Vector2Int fwd = ghostRoverPos + ghostRoverForward;
        controller.roverTileForward = controller.levelTilemap.GetTile(new Vector3Int(fwd.x, fwd.y, 0));

        // Слева
        Vector2Int lft = ghostRoverPos + RotateLeft(ghostRoverForward);
        controller.roverTileLeft = controller.levelTilemap.GetTile(new Vector3Int(lft.x, lft.y, 0));

        // Справа
        Vector2Int rgt = ghostRoverPos + RotateRight(ghostRoverForward);
        controller.roverTileRight = controller.levelTilemap.GetTile(new Vector3Int(rgt.x, rgt.y, 0));

        // Задать переменные
        canMoveForward = controller.roverTileForward != null && controller.roverTileForward.name != controller.emptyTile.name;
        canMoveLeft = controller.roverTileLeft != null && controller.roverTileLeft.name != controller.emptyTile.name;
        canMoveRight = controller.roverTileRight != null && controller.roverTileRight.name != controller.emptyTile.name;
    }

    // Методы, которыми будет пользоваться игрок
    // Важно: при каждом движении и вращении останавливает поток интерпретатора, исполняет действие на главном потоке, и запускает поток.
    public void move()
    {
        if (controller.commandBuffer.Count > maxCommands) throw new Exception("Limit exceeded");

        using (var waitHandle = new ManualResetEventSlim(false))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                ghostRoverPos += ghostRoverForward;
                controller.AddMoveForwardCommand();
                UpdateTiles();
                waitHandle.Set();
            });
            waitHandle.Wait(); // Block Python thread until Main Thread finishes UpdateTiles
        }
    }

    public void turn_left()
    {
        if (controller.commandBuffer.Count > maxCommands) throw new Exception("Limit exceeded");

        using (var waitHandle = new ManualResetEventSlim(false))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                ghostRoverForward = RotateLeft(ghostRoverForward);
                controller.AddTurnLeftCommand();
                UpdateTiles();
                waitHandle.Set();
            });
            waitHandle.Wait();
        }
    }

    public void turn_right()
    {
        if (controller.commandBuffer.Count > maxCommands) throw new Exception("Limit exceeded");

        using (var waitHandle = new ManualResetEventSlim(false))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                ghostRoverForward = RotateRight(ghostRoverForward);
                controller.AddTurnRightCommand();
                UpdateTiles();
                waitHandle.Set();
            });
            waitHandle.Wait();
        }
    }

    public new void print(object text)
    {
        using (var waitHandle = new ManualResetEventSlim(false))
        {
            UnityMainThreadDispatcher.Instance().Enqueue(() => {
                controller.AddPrintDebugCommand(new DebugString { message = text.ToString(), color = debugColor });
                waitHandle.Set();
            });
            waitHandle.Wait();
        }
    }

    // Возврящает данные о ячейках
    public bool can_move_forward() => canMoveForward;
    public bool can_move_left() => canMoveLeft;
    public bool can_move_right() => canMoveRight;

    // Добавляет команду высвечивания ошибки в консоль
    void RaiseError(string message)
    {
        controller.AddPrintDebugCommand(new DebugString { message = message, color = errorColor });
    }
}