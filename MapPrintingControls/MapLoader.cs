using System;
using System.Diagnostics;
using System.Windows.Threading;
using ESRI.ArcGIS.Client;

namespace MapPrintingControls
{
	/// <summary>
	/// Helper class to know when a map is loaded and so ready to print.
	/// It's waiting for progress = 100 but sometimes this event never comes (e.g. with a dynamic layer when the image is in the cache)
	/// So there is a timer to avoid infinite wait.
	/// </summary>
	internal class MapLoader
	{
		#region Contructor
		private readonly Map _map;
		private readonly DispatcherTimer _timer;
		private bool _isProgressing; // no worry : some progress events are coming 

		public MapLoader(Map map)
		{
			Debug.Assert(map != null);
			_map = map;
			_timer = new DispatcherTimer();
			_timer.Tick += Timer_Tick;
			_isProgressing = false;
		}
		#endregion

		#region WaitForLoaded
		/// <summary>
		/// Waits for the map loaded.
		/// </summary>
		public void WaitForLoaded()
		{
			// Wait for map loaded
			_map.Progress += Map_Progress;
			if (_timer.IsEnabled)
				_timer.Stop();
			_isProgressing = false;
			_timer.Interval = TimeSpan.FromSeconds(10); // Wait 10 seconds before the first mapprogress event, after that consider that the map was already ready
			_timer.Start();
		}
		
		#endregion

		#region CancelWait
		/// <summary>
		/// Cancels the wait.
		/// </summary>
		public void CancelWait()
		{
			_timer.Stop();
			_map.Progress -= Map_Progress;
		} 
		#endregion

		#region Event Loaded
		/// <summary>
		/// Occurs when the map is loaded.
		/// </summary>
		public event EventHandler<EventArgs> Loaded;
		private void OnLoaded()
		{
			CancelWait();
			var handler = Loaded;
			if (handler != null)
				handler(this, new EventArgs());
		} 
		#endregion

		#region private void Timer_Tick
		// Security timer to avoid infinite waiting
		private void Timer_Tick(object sender, EventArgs e)
		{
			if (_isProgressing)
			{
				// Progress events are coming -> wait more
				_isProgressing = false;
				_timer.Interval = TimeSpan.FromSeconds(30);
			}
			else
			{
				// No progress event since last test --> stop and consider the map as loaded
				Debug.WriteLine("Warning : MapLoader stopped by timer");
				OnLoaded();
			}
		}
		#endregion

		#region private void Map_Progress
		private void Map_Progress(object sender, ProgressEventArgs e)
		{
			_isProgressing = true;
			Debug.WriteLine("map_progress " + e.Progress);
			if (e.Progress == 100)
				OnLoaded(); // map is ready
		}
		#endregion
	}
}
