using System.Printing;
using System.Windows.Controls;
using System.Windows.Documents;

namespace MapPrintingControls.WPF
{

	/// <summary>
	/// Subclass DocumentViewer to force the usage of a specific printer (could have been done by templating the DocumentViewer)
	/// </summary>
	internal class PrintDocumentViewer : DocumentViewer
	{
		/// <summary>
		/// Gets or sets the print queue.
		/// </summary>
		/// <value>The print queue.</value>
		public PrintQueue PrintQueue { get; set; }

		/// <summary>
		/// Gets or sets the print ticket.
		/// </summary>
		/// <value>The print ticket.</value>
		public PrintTicket PrintTicket { get; set; }

		/// <summary>
		/// Handles the <see cref="P:System.Windows.Input.ApplicationCommands.Print"/> routed command.
		/// Overrides the print command in order to print without printer choice (if PrintQueue initialized)
		/// </summary>
		protected override void OnPrintCommand()
		{
			System.Diagnostics.Debug.Assert(Document != null);
			var fixedDocumentSequence = Document as FixedDocumentSequence;

			if (PrintQueue != null && fixedDocumentSequence != null)
			{
				// Print on a specific printer
				var writer = PrintQueue.CreateXpsDocumentWriter(PrintQueue);
				writer.Write(fixedDocumentSequence, PrintTicket);
			}
			else
			{
				// User selects the printer
				base.OnPrintCommand();
			}
		}
	}
}
