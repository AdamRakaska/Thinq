﻿using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using ThinqCore;
using System.Numerics;

namespace ThinqGUI
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		public void SetControlsStatus(bool IsEnabled)
		{
			groupCoprime.Enabled = IsEnabled;
			btnEnumerateCoFactors.Enabled = IsEnabled;
			if (IsEnabled)
			{
				if (!btnCancelEnumerateCoFactors.Enabled)
				{
					btnCancelEnumerateCoFactors.Enabled = true;
				}
				if (!btnCancelEnumerateCoFactors.Visible)
				{
					btnCancelEnumerateCoFactors.Visible = true;
				}
			}
			else
			{
				btnCancelEnumerateCoFactors.Enabled = false;
				btnCancelEnumerateCoFactors.Visible = false;
			}
		}

		#region CoPrime Enumeration

		public BigInteger CoprimeTo { get { return tbCoPrimeTo.ToBigInteger(); } }
		public BigInteger CoprimeMin { get { return tbCoPrimeMin.ToBigInteger(); } }
		public BigInteger CoprimeMax { get { return tbCoPrimeMax.ToBigInteger(); } }

		private void btnCoprimes_Click(object sender, EventArgs e)
		{
			DateTime startTime = DateTime.UtcNow;
			Coprimes coprimeFinder = new Coprimes(CoprimeTo, CoprimeMin, CoprimeMax);
			List<BigInteger> coPrimes = coprimeFinder.GetCoprimes().ToList();
			string joinedCoprimes = string.Join(Environment.NewLine, coPrimes);
			TimeSpan coprimesTimeElapsed = DateTime.UtcNow.Subtract(startTime);

			coprimesTimeElapsed.ToString("m'm 's's.'FFFFFFF");

			StringBuilder resultString = new StringBuilder();
			resultString.AppendFormat("Total # of co-primes found in range: {0}", coPrimes.Count);
			resultString.AppendLine();
			resultString.AppendFormat("Total time elapsed: {0}", coprimesTimeElapsed.ToString("m'm 's's.'ffff"));
			resultString.AppendLine();
			resultString.Append(joinedCoprimes);

			tbOutput.Clear();
			DisplayOutput(resultString.ToString());
			tbOutput.Select(0, 0);
			tbOutput.ScrollToCaret();
		}

		private void btnEnumeratePrimeFactors_Click(object sender, EventArgs e)
		{
			DateTime startTime = DateTime.UtcNow;
			Coprimes factorsFinder = new Coprimes(CoprimeTo, CoprimeMin, CoprimeMax);
			List<BigInteger> factors = factorsFinder.GetPrimeFactors().ToList();
			string joinedFactors = string.Join(Environment.NewLine, factors);
			TimeSpan factorsTimeElapsed = DateTime.UtcNow.Subtract(startTime);

			factorsTimeElapsed.ToString("m'm 's's.'FFFFFFF");

			StringBuilder resultString = new StringBuilder();
			resultString.AppendFormat("Total # of prime found in range: {0}", factors.Count);
			resultString.AppendLine();
			resultString.AppendFormat("Total time elapsed: {0}", factorsTimeElapsed.ToString("m'm 's's.'ffff"));
			resultString.AppendLine();
			resultString.Append(joinedFactors);

			tbOutput.Clear();
			DisplayOutput(resultString.ToString());
			tbOutput.Select(0, 0);
			tbOutput.ScrollToCaret();
		}

		#endregion

		#region CoFactor Enumeration

		public BigInteger ResultMinValue { get { return tbResultMinValue.ToBigInteger(); } }
		public BigInteger ResultMaxValue { get { return tbResultMaxValue.ToBigInteger(); } }
		public BigInteger ResultMaxQuantity { get { return tbResultMaxQuantity.ToBigInteger(); } }
		public List<BigInteger> CoFactors { get { return listCoFactors.Items.Cast<string>().Select(s => BigInteger.Parse(TryParse.RemoveNonDigits(s))).ToList(); } }

		private AsyncBackgroundTask backgroundTask = null;

		private void btnEnumerateCoFactors_Click(object sender, EventArgs e)
		{
			Console.Clear();
			tbOutput.Clear();
			btnCancelEnumerateCoFactors.Visible = true;
			btnCancelEnumerateCoFactors.Enabled = true;
			EnumerateCoFactors();
		}

		private void EnumerateCoFactors()
		{
			if (CoFactors.Count < 1
				|| CoFactors.Any(i => i < 2)
				//|| CoFactors.Any(i => i < CoFactorMin)
				|| CoFactors.Any(i => i > ResultMaxValue)
				|| ResultMinValue >= ResultMaxValue
				|| ResultMinValue < 2
				|| ResultMaxValue < 2)
			{
				return;
			}

			if (backgroundTask != null)
			{
				if (!backgroundTask.IsDisposed)
				{
					if (backgroundTask.IsBusy)
					{
						return;
					}
					backgroundTask.Dispose();
				}
				backgroundTask = null;
			}

			backgroundTask = new AsyncBackgroundTask();
			if (backgroundTask != null)
			{
				SetControlsStatus(false);
				backgroundTask.RunWorkerAsync();
			}
		}

		private void btnEnumerateGCD_Click(object sender, EventArgs e)
		{
			IEnumerable<BigInteger> numbers = CoFactors.Select(bi => (BigInteger)bi);
			DisplayOutput("GCD[{0}]:", string.Join(", ", numbers));
			DisplayOutput("{0}", Coprimes.FindGCD(numbers));
			DisplayOutput("");
		}

		private void btnEnumerateLCM_Click(object sender, EventArgs e)
		{
			IEnumerable<BigInteger> numbers = CoFactors.Select(bi => (BigInteger)bi);
			DisplayOutput("LCM[{0}]:", string.Join(", ", numbers));
			DisplayOutput("{0}", Coprimes.FindLCM(numbers));
			DisplayOutput("");
		}

		private void btnAddCoFactor_Click(object sender, EventArgs e)
		{
			AddNewCoFactor();
		}

		private void btnCancelEnumerateCoFactors_Click(object sender, EventArgs e)
		{
			if (backgroundTask.CancelAsync())
			{
				btnCancelEnumerateCoFactors.Enabled = false;
			}
		}

		private void tbCoFactorAdd_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyData == Keys.Enter)
			{
				AddNewCoFactor();
			}
		}

		private void listCoFactors_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Delete)
			{
				RemoveSelectedCoFactors();
			}
		}

		private void listCoFactors_menuDelete_Click(object sender, EventArgs e)
		{
			RemoveSelectedCoFactors();
		}

		private void AddNewCoFactor()
		{
			BigInteger newCofactor = tbCoFactorAdd.ToBigInteger();
			if (newCofactor > 1 && !CoFactors.Contains(newCofactor))
			{
				tbCoFactorAdd.Text = string.Empty;
				listCoFactors.Items.Add(newCofactor.ToString()); //FindFactorsFromListBox();
				tbCoFactorAdd.Select(); // Put the cursor back in the TextBox to smooth experience for adding another number
			}
		}

		private void RemoveSelectedCoFactors()
		{
			int lastIndex = -1;
			List<object> selectedListItems = listCoFactors.SelectedItems.OfType<object>().ToList();
			foreach (object item in selectedListItems)
			{
				lastIndex = listCoFactors.Items.IndexOf(item);
				listCoFactors.Items.Remove(item);
			}

			// Limit lastIndex to within range
			if (lastIndex > listCoFactors.Items.Count - 1)
			{
				lastIndex = listCoFactors.Items.Count - 1;
			}

			// Check for valid lastIndex
			if (lastIndex > -1)
			{
				// Highlight next item after deleted item. This creates a smoother keyboard experience
				listCoFactors.SetSelected(lastIndex, true);
			}
		}

		#endregion

		#region Output TextBox

		public void DisplayOutput(string format, params object[] args)
		{
			if (tbOutput.InvokeRequired)
			{
				tbOutput.Invoke(new MethodInvoker(() => DisplayOutput(format, args)));
			}
			else
			{
				StringBuilder newText = new StringBuilder();
				if (!string.IsNullOrEmpty(format))
				{
					if (args == null || args.Length < 1)
					{
						newText.Append(format);
					}
					else
					{
						newText.AppendFormat(format, args);
					}
				}
				newText.AppendLine(); //if (!newText.ToString().EndsWith(Environment.NewLine))
				tbOutput.AppendText(newText.ToString());
			}
		}

		private void tbOutput_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.Control)
			{
				if (e.KeyCode == Keys.A) // CTRL + A, Select all
				{
					tbOutput.SelectAll();
				}
				else if (e.KeyCode == Keys.S) // CTRL + S, Save as
				{
					using (SaveFileDialog saveFileDialog = new SaveFileDialog())
					{
						if (saveFileDialog.ShowDialog() == DialogResult.OK)
						{
							string filename = saveFileDialog.FileName;
							File.WriteAllLines(filename, tbOutput.Lines);
						}
					}
				}
			}
		}

		#endregion
	}
}
