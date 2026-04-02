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

            // 1. Processador
            using var searcherProcessor = new ManagementObjectSearcher("select Name from Win32_Processor");
            foreach (ManagementObject obj in searcherProcessor.Get())
            {
                table.AddRow("Processador", obj["Name"]?.ToString() ?? "N/A");
            }

            // 2. Memória RAM
            using var searcherComputer = new ManagementObjectSearcher("Select TotalPhysicalMemory From Win32_ComputerSystem");
            foreach (ManagementObject obj in searcherComputer.Get())
            {
                double ramGb = Convert.ToDouble(obj["TotalPhysicalMemory"]) / (1024 * 1024 * 1024);
                table.AddRow("Memória RAM", $"{Math.Round(ramGb, 2)} GB");
            }

            // 3. SO
            using var searcherOS = new ManagementObjectSearcher("Select Caption From Win32_OperatingSystem");
            foreach (ManagementObject obj in searcherOS.Get())
            {
                table.AddRow("SO", obj["Caption"]?.ToString() ?? "N/A");
            }

            return table;
        }
    }
}