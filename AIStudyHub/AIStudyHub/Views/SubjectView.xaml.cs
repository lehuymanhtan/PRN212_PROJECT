using System;
using System.Windows.Controls;

namespace AIStudyHub.Views
{
    public partial class SubjectView : UserControl
    {
        public SubjectView()
        {
            InitializeComponent();
        }

        private void DataGrid_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.Row.Item is Models.Subject subject)
                {
                    // Đợi luồng UI cập nhật giá trị mới từ ô cell vào object
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            using var db = new Data.AppDbContext();
                            var existing = db.Subjects.Find(subject.Id);
                            if (existing != null)
                            {
                                existing.Name = subject.Name;
                                existing.CourseCode = subject.CourseCode;
                                db.SaveChanges();
                            }
                        }
                        catch { }
                    }), System.Windows.Threading.DispatcherPriority.Background);
                }
            }
        }
    }
}
