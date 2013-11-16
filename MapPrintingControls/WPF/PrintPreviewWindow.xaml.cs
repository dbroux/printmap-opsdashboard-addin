using System.Windows.Documents;

namespace MapPrintingControls.WPF
{
	/// <summary>
	/// Print preview an XPS document (wrapper to a window including a document viewer)
	/// </summary>
	internal partial class PrintPreviewWindow
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="PrintPreviewWindow"/> class.
		/// </summary>
		public PrintPreviewWindow()
		{
			InitializeComponent();
		}

		public System.Printing.PrintQueue PrintQueue
		{
			get { return viewer.PrintQueue; }
			set { viewer.PrintQueue = value; }
		}

		public System.Printing.PrintTicket PrintTicket
		{
			get { return viewer.PrintTicket; }
			set { viewer.PrintTicket = value; }
		}

		/// <summary>
		/// Gets or sets the document.
		/// </summary>
		/// <value>The document.</value>
		public IDocumentPaginatorSource Document
		{
		    get { return viewer.Document; }
		    set { viewer.Document = value; }
		}
	}
}
