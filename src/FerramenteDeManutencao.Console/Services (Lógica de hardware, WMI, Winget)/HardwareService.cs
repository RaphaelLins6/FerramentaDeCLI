using System.Management;
using System.Runtime.Versioning;
using Spectre.Console;
using System.Runtime.InteropServices;

namespace ToolManutencao.Services
{
    [SupportedOSPlatform("windows")]
    public class HardwareService
    {
        private readonly bool _isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        public Table GetHardwareTable()
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("[bold blue]Componente[/]");
            table.AddColumn("[bold blue]Detalhes[/]");

            if (_isWindows)
            {
                ObterDadosWindows(table);
            }
            else
            {
                ObterDadosLinux(table);
            }

            return table;
        }

        private void ObterDadosWindows(Table table)
        {
            // 1. SO
            using var searcherOS = new ManagementObjectSearcher("Select Caption From Win32_OperatingSystem");
            foreach (ManagementObject obj in searcherOS.Get())
            {
                table.AddRow("SO", obj["Caption"]?.ToString() ?? "N/A");
            }

            // 2. Processador
            using var searcherProcessor = new ManagementObjectSearcher("select Name from Win32_Processor");
            foreach (ManagementObject obj in searcherProcessor.Get())
            {
                table.AddRow("Processador", obj["Name"]?.ToString() ?? "N/A");
            }

            // 3. Memória RAM e Tipo DDR
            using var searcherRAM = new ManagementObjectSearcher("Select Capacity, Speed, MemoryType, SMBIOSMemoryType From Win32_PhysicalMemory");
            string ddrTipo = "RAM";
            double totalCapacity = 0;

            foreach (ManagementObject obj in searcherRAM.Get())
            {
                totalCapacity += Convert.ToDouble(obj["Capacity"]);
                
                // O SMBIOSMemoryType é o padrão mais moderno para detectar DDR4/DDR5
                uint typeCode = Enumerable.Range(0, 1).Select(_ => {
                    var t = obj["SMBIOSMemoryType"];
                    return t != null ? Convert.ToUInt32(t) : 0;
                }).First();

                ddrTipo = typeCode switch
                {
                    20 => "DDR",
                    21 => "DDR2",
                    24 => "DDR3",
                    26 => "DDR4",
                    34 => "DDR5",
                    _ => "DDR" 
                };
            }

            double ramGb = totalCapacity / (1024.0 * 1024.0 * 1024.0);
            table.AddRow("Memória RAM", $"{Math.Ceiling(ramGb)} GB {ddrTipo}");

            // 4. Armazenamento (HDD, SSD, NVMe)
            using var searcherDisk = new ManagementObjectSearcher("Select Model, Size From Win32_DiskDrive");
            foreach (ManagementObject obj in searcherDisk.Get())
            {
                ulong bytes = (ulong)(obj["Size"] ?? 0);
                double sizeGb = bytes / (1024.0 * 1024.0 * 1024.0);
                string modelo = obj["Model"]?.ToString() ?? "Desconhecido";

                // Lógica para detectar o tipo pelo nome do modelo
                string tipo = "HDD"; // Padrão
                string modeloUpper = modelo.ToUpper();

                if (modeloUpper.Contains("SSD") || modeloUpper.Contains("NVME") || modeloUpper.Contains("SATA") && sizeGb < 600) 
                    tipo = "SSD";
                if (modeloUpper.Contains("NVME")) 
                    tipo = "NVMe";

                // Sua lógica de tamanho comercial (mantida)
                string tamanhoComercial;
                if (sizeGb > 900) tamanhoComercial = "1 TB";
                else if (sizeGb > 440 && sizeGb < 500) tamanhoComercial = "480 GB";
                else if (sizeGb > 220 && sizeGb < 250) tamanhoComercial = "240 GB";
                else tamanhoComercial = $"{Math.Round(sizeGb, 0)} GB";

                // Adiciona na tabela com o tipo entre parênteses
                table.AddRow("Disco", $"{modelo} ({tipo}) - {tamanhoComercial}");
            }

            // 5. GPU (Placa de Vídeo)
            using var searcherGPU = new ManagementObjectSearcher("Select Name, AdapterRAM From Win32_VideoController");
            foreach (ManagementObject obj in searcherGPU.Get())
            {
                string nomeGpu = obj["Name"]?.ToString() ?? "N/A";
                
                // O AdapterRAM pode retornar valores imprecisos em placas modernas, 
                // mas serve como uma base para memória dedicada.
                var bytesGpu = obj["AdapterRAM"] != null ? Convert.ToInt64(obj["AdapterRAM"]) : 0;
                double vramGb = Math.Abs(bytesGpu) / (1024.0 * 1024.0 * 1024.0);

                string infoVram = vramGb > 0 ? $" ({Math.Round(vramGb, 0)} GB VRAM)" : "";
                table.AddRow("GPU", $"{nomeGpu}{infoVram}");
            }
        }

        private void ObterDadosLinux(Table table)
        {
            // 1. SO
            if (File.Exists("/etc/os-release"))
            {
                var osInfo = File.ReadAllLines("/etc/os-release")
                    .FirstOrDefault(l => l.StartsWith("PRETTY_NAME="))?
                    .Replace("PRETTY_NAME=", "").Replace("\"", "");
                table.AddRow("SO", osInfo ?? "Linux");
            }

            // 2. Processador
            if (File.Exists("/proc/cpuinfo"))
            {
                var cpuLine = File.ReadAllLines("/proc/cpuinfo")
                    .FirstOrDefault(l => l.StartsWith("model name"))?
                    .Split(':').Last().Trim();
                table.AddRow("Processador", cpuLine ?? "N/A");
            }

            // 3. Memória RAM
            if (File.Exists("/proc/meminfo"))
            {
                var memLine = File.ReadAllLines("/proc/meminfo")
                    .FirstOrDefault(l => l.StartsWith("MemTotal"));
                if (memLine != null)
                {
                    var totalKb = double.Parse(System.Text.RegularExpressions.Regex.Match(memLine, @"\d+").Value);
                    var totalGb = Math.Ceiling(totalKb / (1024 * 1024));
                    table.AddRow("Memória RAM", $"{totalGb} GB");
                }
            }

            // 4. Discos no Linux
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
            foreach (var drive in drives)
            {
                try 
                {
                    double totalSize = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                    table.AddRow("Disco (Montado)", $"{drive.Name} - {Math.Round(totalSize, 0)} GB");
                }
                catch (UnauthorizedAccessException)
                {
                    // Ignora discos que o usuário atual não tem permissão para ler o tamanho
                    continue; 
                }
                catch (IOException)
                {
                    // Ignora erros de operação não permitida (comum em sistemas de arquivos virtuais)
                    continue;
                }
            }

            // 5. GPU (Linux)
            try
            {
                // Tenta listar dispositivos de vídeo via barramento PCI
                var gpuDevices = Directory.GetDirectories("/sys/bus/pci/devices");
                bool gpuEncontrada = false;

                foreach (var dev in gpuDevices)
                {
                    string classFile = Path.Combine(dev, "class");
                    if (File.Exists(classFile))
                    {
                        string classId = File.ReadAllText(classFile).Trim();
                        // 0x030000 é a classe para Display Controller (GPU)
                        if (classId.StartsWith("0x0300"))
                        {
                            // Tenta ler o arquivo de 'uevent' que costuma conter o driver ou ID
                            string ueventPath = Path.Combine(dev, "uevent");
                            if (File.Exists(ueventPath))
                            {
                                var driverLine = File.ReadAllLines(ueventPath)
                                    .FirstOrDefault(l => l.StartsWith("DRIVER="))?
                                    .Split('=').Last();

                                table.AddRow("GPU", $"Dispositivo PCI ({driverLine ?? "Genérico"})");
                                gpuEncontrada = true;
                                break;
                            }
                        }
                    }
                }

                if (!gpuEncontrada)
                {
                    // Alternativa via comando shell rápido caso o sysfs falhe
                    var process = new System.Diagnostics.Process
                    {
                        StartInfo = new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = "/bin/bash",
                            Arguments = "-c \"lspci | grep -i vga | cut -d ':' -f3\"",
                            RedirectStandardOutput = true,
                            UseShellExecute = false,
                            CreateNoWindow = true
                        }
                    };
                    process.Start();
                    string result = process.StandardOutput.ReadToEnd().Trim();
                    if (!string.IsNullOrEmpty(result)) table.AddRow("GPU", result);
                    else table.AddRow("GPU", "Não detectada");
                }
            }
            catch
            {
                table.AddRow("GPU", "Erro ao detectar");
            }
        }

        public void ExibirMenuTestes()
        {
            bool voltar = false;
            while (!voltar)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new FigletText("Testes").Centered().Color(Color.Yellow));
                AnsiConsole.WriteLine();

                var opt = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[yellow]Selecione o teste desejado:[/]")
                        .AddChoices("Saúde dos Discos (S.M.A.R.T)", "Saúde da Memória RAM", "Voltar"));

                switch (opt)
                {
                    case "Saúde dos Discos (S.M.A.R.T)":
                        ExibirSaudeDiscos();
                        AnsiConsole.MarkupLine("\n[grey]Pressione qualquer tecla para voltar...[/]");
                        Console.ReadKey();
                        break;

                    case "Saúde da Memória RAM":
                        VerificarSaudeRAM();
                        AnsiConsole.MarkupLine("\n[grey]Pressione qualquer tecla para voltar...[/]");
                        Console.ReadKey();
                        break;

                    case "Voltar":
                        voltar = true;
                        break;
                }
            }
        }

        public void ExibirSaudeDiscos()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new FigletText("Saude Disco").Centered().Color(Color.Blue1));
            AnsiConsole.Write(new Rule("[yellow]Teste de Integridade de Disco (S.M.A.R.T)[/]").RuleStyle("grey").Justify(Justify.Left));

            try
            {
                // 1. Consulta básica de discos
                using var searcher = new ManagementObjectSearcher("SELECT Model, Status, Size, DeviceID FROM Win32_DiskDrive");
                var discos = searcher.Get();

                if (discos.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]Nenhum disco detectado.[/]");
                    return;
                }

                var table = new Table().Border(TableBorder.Rounded).Expand();
                table.AddColumn("[bold]Modelo[/]");
                table.AddColumn(new TableColumn("[bold]Status Geral[/]").Centered());
                table.AddColumn(new TableColumn("[bold]Bad Blocks (Realocados)[/]").Centered());
                table.AddColumn(new TableColumn("[bold]Tamanho[/]").Centered());

                foreach (ManagementObject drive in discos)
                {
                    string modelo = drive["Model"]?.ToString() ?? "Desconhecido";
                    string statusRaw = drive["Status"]?.ToString() ?? "N/A";
                    string deviceId = drive["DeviceID"]?.ToString() ?? "";

                    // Lógica de Status Geral
                    string statusFormatado = statusRaw.ToUpper() switch
                    {
                        "OK" => "[green]Saudável[/]",
                        "PRED FAIL" => "[red]FALHA IMINENTE[/]",
                        _ => $"[yellow]{statusRaw}[/]"
                    };

                    // 2. Busca de Bad Blocks (Setores Realocados) via Root\WMI
                    int badBlocksEncontrados = 0;
                    try
                    {
                        using var searcherAtributos = new ManagementObjectSearcher(@"Root\WMI", "SELECT VendorSpecific FROM MSStorageDriver_FailurePredictData");
                        foreach (ManagementObject data in searcherAtributos.Get())
                        {
                            byte[] vendorSpecific = (byte[])data["VendorSpecific"];
                            // Atributo 05 é o padrão para Reallocated Sectors Count
                            for (int i = 0; i < vendorSpecific.Length; i += 12)
                            {
                                if (vendorSpecific[i] == 0x05) 
                                {
                                    badBlocksEncontrados = vendorSpecific[i + 5]; 
                                }
                            }
                        }
                    }
                    catch { /* Alguns drives/pendrives não suportam leitura bruta de atributos */ }

                    ulong bytes = (ulong)(drive["Size"] ?? 0);
                    double sizeGb = bytes / (1024.0 * 1024.0 * 1024.0);

                    string badBlockDisplay = badBlocksEncontrados > 0 
                        ? $"[red]{badBlocksEncontrados}[/]" 
                        : "[green]0[/]";

                    table.AddRow(modelo, statusFormatado, badBlockDisplay, $"{Math.Round(sizeGb, 0)} GB");
                }

                AnsiConsole.Write(table);
                
                var desejaExplicacao = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[cyan]Deseja ver uma explicação sobre Bad Blocks?[/]")
                        .AddChoices("Sim", "Não")) == "Sim";

                if (desejaExplicacao)
                {
                    AnsiConsole.MarkupLine("\n[grey]INFO:[/] [white]Bad Blocks (Setores Realocados)[/] indicam que o disco encontrou erros físicos e moveu os dados para uma área de reserva.");
                    AnsiConsole.MarkupLine("[grey]DICA:[/] Se o número estiver acima de [red]0[/] e subindo, faça backup dos seus dados imediatamente.");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Erro ao acessar dados de disco:[/] {ex.Message}");
            }
        }

        public void VerificarSaudeRAM()
        {
            AnsiConsole.Clear();
            // Título Figlet para padronizar o visual
            AnsiConsole.Write(new FigletText("Saude RAM").Centered().Color(Color.Green1));
            AnsiConsole.Write(new Rule("[yellow]Analise de Integridade da Memoria[/]").LeftJustified());

            try
            {
                // Usando Win32_PhysicalMemory para maior compatibilidade com DDR4/DDR5
                using var searcher = new ManagementObjectSearcher("SELECT BankLabel, Capacity, Speed, MemoryType, FormFactor FROM Win32_PhysicalMemory");
                
                var table = new Table().Border(TableBorder.Rounded).Expand();
                table.AddColumn("Slot/Banco");
                table.AddColumn(new TableColumn("Capacidade").Centered());
                table.AddColumn(new TableColumn("Velocidade").Centered());
                table.AddColumn(new TableColumn("Status (Hardware)").Centered());

                var pentes = searcher.Get();
                if (pentes.Count == 0)
                {
                    AnsiConsole.MarkupLine("[yellow]Nenhum pente de memoria fisica detectado pelo WMI.[/]");
                    return;
                }

                foreach (ManagementObject ram in pentes)
                {
                    string banco = ram["BankLabel"]?.ToString() ?? "N/A";
                    ulong bytes = (ulong)(ram["Capacity"] ?? 0);
                    string velocidade = ram["Speed"]?.ToString() ?? "--";

                    // No Windows, se o pente aparece aqui, ele passou no teste elétrico inicial
                    string statusFormatado = "[green]Saudavel (Ativo)[/]";

                    table.AddRow(banco, $"{bytes / (1024 * 1024 * 1024)} GB", $"{velocidade} MHz", statusFormatado);
                }

                AnsiConsole.Write(table);

                // Explicação opcional em Português
                var desejaExplicacao = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("\n[cyan]Deseja entender como a saude da RAM e testada?[/]")
                        .AddChoices("Sim", "Não")) == "Sim";

                if (desejaExplicacao)
                {
                    AnsiConsole.MarkupLine("\n[grey]INFO:[/] Diferente dos discos, a RAM não possui contadores de erro preventivos (S.M.A.R.T).");
                    AnsiConsole.MarkupLine("[grey]STATUS:[/] [green]Saudavel (Ativo)[/] significa que o pente foi reconhecido e passou no autoteste (POST) da placa-mãe.");
                    AnsiConsole.MarkupLine("[grey]DICA:[/] Se o PC apresenta [blue]Telas Azuis[/] constantes, use o comando [yellow]mdsched.exe[/] para um teste de estresse profundo.");
                }
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Erro ao acessar dados da RAM:[/] {ex.Message}");
            }
        }
    }
}