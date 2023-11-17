namespace VpmfServiceManager
{
	partial class ProjectInstaller
	{
		/// <summary>
		/// Variabile di progettazione necessaria.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Pulire le risorse in uso.
		/// </summary>
		/// <param name="disposing">ha valore true se le risorse gestite devono essere eliminate, false in caso contrario.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Codice generato da Progettazione componenti

		/// <summary>
		/// Metodo necessario per il supporto della finestra di progettazione. Non modificare
		/// il contenuto del metodo con l'editor di codice.
		/// </summary>
		private void InitializeComponent()
		{
			this.VpmfServiceManagerInstaller = new System.ServiceProcess.ServiceProcessInstaller();
			this.VpmfServiceManager = new System.ServiceProcess.ServiceInstaller();
			// 
			// VpmfServiceManagerInstaller
			// 
			this.VpmfServiceManagerInstaller.Password = null;
			this.VpmfServiceManagerInstaller.Username = null;
			// 
			// VpmfServiceManager
			// 
			this.VpmfServiceManager.DisplayName = "VpmfServiceManager";
			this.VpmfServiceManager.ServiceName = "VpmfServiceManager";
			// 
			// ProjectInstaller
			// 
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {
            this.VpmfServiceManagerInstaller,
            this.VpmfServiceManager});

		}

		#endregion

		private System.ServiceProcess.ServiceProcessInstaller VpmfServiceManagerInstaller;
		private System.ServiceProcess.ServiceInstaller VpmfServiceManager;
	}
}