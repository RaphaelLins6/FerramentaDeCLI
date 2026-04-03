using System.Management;
using System.Runtime.Versioning;
using Spectre.Console;

namespace ToolManutencao.Services
{
    [SupportedOSPlatform("windows")]
    public class HardwareService
    {
        public Table GetHardwareTable()
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("[bold blue]Componente[/]");
            table.AddColumn("[bold blue]Detalhes[/]");

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

            // 3. Memória RAM
            using var searcherComputer = new ManagementObjectSearcher("Select TotalPhysicalMemory From Win32_ComputerSystem");
            foreach (ManagementObject obj in searcherComputer.Get())
            {
                double ramGb = Convert.ToDouble(obj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024);
                table.AddRow("Memória RAM", $"{Math.Round(ramGb, 2)} GB");
            }

            // 4. Armazenamento (HDD, SSD, NVMe)
            using var searcherDisk = new ManagementObjectSearcher("Select Model, Size From Win32_DiskDrive");
            foreach (ManagementObject obj in searcherDisk.Get())
            {
                // O tamanho vem em Bytes, vamos converter para GB para ficar legível
                ulong bytes = (ulong)(obj["Size"] ?? 0);
                double sizeGb = bytes / (1024.0 * 1024.0 * 1024.0);
                
                string modelo = obj["Model"]?.ToString() ?? "Desconhecido";
                
                table.AddRow("Disco", $"{modelo} ({Math.Round(sizeGb, 0)} GB)");
            }

            return table;
        }

        public void ExibirSaudeDiscos()
        {
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