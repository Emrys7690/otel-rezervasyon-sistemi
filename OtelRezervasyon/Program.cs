using OtelRezervasyon.Data;
using OtelRezervasyon.Forms;

namespace OtelRezervasyon;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        try
        {
            DatabaseManager.VeritabaniniHazirla();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Veritabanı başlatılamadı:\n\n{ex.Message}",
                "Başlatma hatası",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        Application.Run(new MainForm());
    }
}
