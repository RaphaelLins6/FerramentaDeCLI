using Spectre.Console;
using ToolManutencao.Services;

AnsiConsole.Clear();
AnsiConsole.Write(new FigletText("Service Tool").Centered().Color(Color.Blue));

// Instanciando a classe de serviço (POO)
var hardwareService = new HardwareService();

AnsiConsole.Status()
    .Start("Escaneando hardware...", ctx => 
    {
        var table = hardwareService.GetHardwareTable();
        AnsiConsole.Write(table);
    });

AnsiConsole.MarkupLine("\n[grey]Pressione qualquer tecla para sair...[/]");
Console.ReadKey();
