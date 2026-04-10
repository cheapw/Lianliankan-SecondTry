using System;
using System.Collections.Generic;
using System.Text;

namespace Lianliankan_WinUI.Messages
{
	public class DifficultySliderChangeMessage
	{
		public int Rows { get; set; }
		public int Columns { get; set; }
		public DifficultySliderChangeMessage(int rows,int columns)
		{
			Rows = rows;
			Columns = columns;
		}
	}
}
