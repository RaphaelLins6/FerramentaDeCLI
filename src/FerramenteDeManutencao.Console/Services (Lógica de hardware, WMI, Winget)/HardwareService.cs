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

            // 4. Discos
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);
            foreach (var drive in drives)
            {
                double totalSize = drive.TotalSize / (1024.0 * 1024.0 * 1024.0);
                table.AddRow("Disco (Montado)", $"{drive.Name} - {Math.Round(totalSize, 0)} GB");
            }
        }

        public void ExibirSaudeDiscos()
        {
            if (!_isWindows)
            {
                AnsiConsole.MarkupLine("[red]Teste S.M.A.R.T via WMI só está disponível no Windows.[/]");
                AnsiConsole.MarkupLine("[grey]No Linux, instale o 'smartmontools' e use 'sudo smartctl -a /dev/sda'.[/]");
                return;
            }
            
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[yellow]Teste de Integridade de Disco (S.M.A.R.T)[/]").RuleStyle("grey").Justify(Justify.Left));

            try
            {
                // Consulta o armazenamento físico (Requer privilégios de Administrador)
                using var searcher = new ManagementObjectSearcher(@"Root\Microsoft\Windows\Storage", "SELECT * FROM MSFT_PhysicalDisk");
                
                var table = new Table().Border(TableBorder.Rounded).Expand();
                table.AddColumn("[bold]Modelo[/]");
                table.AddColumn(new TableColumn("[bold]Status de Saúde[/]").Centered());
                table.AddColumn(new TableColumn("[bold]Temperatura[/]").Centered());
                table.AddColumn(new TableColumn("[bold]Mídia[/]").Centered());

                foreach (ManagementObject drive in searcher.Get())
                {
                    string nome = drive["FriendlyName"]?.ToString() ?? "Desconhecido";
                    string saude = drive["HealthStatus"]?.ToString() ?? "N/A";
                    string temp = drive["Temperature"] != null ? $"{drive["Temperature"]}°C" : "--";
                    
                    // Identifica se é SSD ou HDD (MediaType: 3 = HDD, 4 = SSD)
                    uint mediaType = (uint)(drive["MediaType"] ?? 0);
                    string tipo = mediaType == 4 ? "[blue]SSD[/]" : (mediaType == 3 ? "[yellow]HDD[/]" : "Outro");

                    // Define a cor baseada no status
                    string statusFormatado = saude switch
                    {
                        "Healthy" => "[green]Saudável[/]",
                        "Warning" => "[yellow]Aviso (Check-up necessário)[/]",
                        "Unhealthy" => "[red]Crítico (Risco de Perda de Dados)[/]",
                        _ => "[grey]Desconhecido[/]"
                    };

                    table.AddRow(nome, statusFormatado, temp, tipo);
                }

                AnsiConsole.Write(table);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]Erro ao acessar dados de disco:[/] {ex.Message}");
                AnsiConsole.MarkupLine("[grey]Dica: Tente rodar o programa como Administrador.[/]");
            }
        }
    }
}