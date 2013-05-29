using System;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using System.IO;

namespace AzureBlobUploader
{
	public partial class RootViewController : UITableViewController
	{
		DataSource dataSource;

		public UIImagePickerController ImagePicker { get; private set; }

		public RootViewController (IntPtr handle) : base (handle)
		{
			Title = NSBundle.MainBundle.LocalizedString ("Master", "Master");

			// Custom initialization
		}

		void AddNewItem (object sender, EventArgs args)
		{
			var picker = new UIImagePickerController ();

			this.NavigationController.PresentViewController (picker, true, new NSAction(()=>{
				Console.WriteLine("done."); 
			}));

			picker.FinishedPickingMedia += (object s, UIImagePickerMediaPickedEventArgs e) => {
				picker.DismissViewController (true, null);

				var randomInt = Convert.ToUInt64(new Random().NextDouble() * UInt64.MaxValue);
				var name = randomInt + ".png";
				UploadImage(e.OriginalImage, name);
			};

			picker.Canceled += (s, e) => {
				picker.DismissViewController (true, null);
			};

			//dataSource.Objects.Insert (0, DateTime.Now);

			//using (var indexPath = NSIndexPath.FromRowSection (0, 0))
			//	TableView.InsertRows (new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Automatic);
		}

		private void UploadImage(UIImage image, String name) 
		{
			var creds = new StorageCredentials ("YOUR_AZURE_STORAGE_ACCOUNT_NAME", "YOUR_AZURE_STORAGE_ACCOUNT_KEY");
			var client = new CloudStorageAccount (creds, true).CreateCloudBlobClient ();
			var container = client.GetContainerReference("images");
			container.CreateIfNotExists();
			var blob = container.GetBlockBlobReference(name);
			var pngImage = image.AsPNG ();
			var stream = pngImage.AsStream();

			blob.UploadFromStream (stream);

			Console.WriteLine (image.DebugDescription);
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
			NavigationItem.LeftBarButtonItem = EditButtonItem;

			var addButton = new UIBarButtonItem (UIBarButtonSystemItem.Add, AddNewItem);
			NavigationItem.RightBarButtonItem = addButton;

			TableView.Source = dataSource = new DataSource (this);
		}

		class DataSource : UITableViewSource
		{
			static readonly NSString CellIdentifier = new NSString ("DataSourceCell");
			List<object> objects = new List<object> ();
			RootViewController controller;

			public DataSource (RootViewController controller)
			{
				this.controller = controller;
			}

			public IList<object> Objects {
				get { return objects; }
			}
			// Customize the number of sections in the table view.
			public override int NumberOfSections (UITableView tableView)
			{
				return 1;
			}

			public override int RowsInSection (UITableView tableview, int section)
			{
				return objects.Count;
			}
			// Customize the appearance of table view cells.
			public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
			{
				var cell = (UITableViewCell)tableView.DequeueReusableCell (CellIdentifier, indexPath);

				cell.TextLabel.Text = objects [indexPath.Row].ToString ();

				return cell;
			}

			public override bool CanEditRow (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
			{
				// Return false if you do not want the specified item to be editable.
				return true;
			}

			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
			{
				if (editingStyle == UITableViewCellEditingStyle.Delete) {
					// Delete the row from the data source.
					objects.RemoveAt (indexPath.Row);
					controller.TableView.DeleteRows (new NSIndexPath[] { indexPath }, UITableViewRowAnimation.Fade);
				} else if (editingStyle == UITableViewCellEditingStyle.Insert) {
					// Create a new instance of the appropriate class, insert it into the array, and add a new row to the table view.
				}
			}
			/*
			// Override to support rearranging the table view.
			public override void MoveRow (UITableView tableView, NSIndexPath sourceIndexPath, NSIndexPath destinationIndexPath)
			{
			}
			*/

			/*
			// Override to support conditional rearranging of the table view.
			public override bool CanMoveRow (UITableView tableView, NSIndexPath indexPath)
			{
				// Return false if you do not want the item to be re-orderable.
				return true;
			}
			*/
		}

		public override void PrepareForSegue (UIStoryboardSegue segue, NSObject sender)
		{
			if (segue.Identifier == "showDetail") {
				var indexPath = TableView.IndexPathForSelectedRow;
				var item = dataSource.Objects [indexPath.Row];

				((DetailViewController)segue.DestinationViewController).SetDetailItem (item);
			}
		}
	}
}

