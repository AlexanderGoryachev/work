using Microsoft.Win32;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace CreatingRecipeApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MobileServiceClient MobileService = new MobileServiceClient(constants.AppUrl, constants.AppKey);

        public MainWindow()
        {
            InitializeComponent();
        }

        private void RecipeGroupIdComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<Group> groups = new List<Group>();
            groups.Add(new Group { Title = "Закуски", Id = 1 });
            groups.Add(new Group { Title = "Первое", Id = 2 });
            groups.Add(new Group { Title = "Напитки", Id = 3 });
            groups.Add(new Group { Title = "Салаты", Id = 6 });
            groups.Add(new Group { Title = "Второе", Id = 7 });
            groups.Add(new Group { Title = "Десерты", Id = 8 });

            var comboBox = sender as ComboBox;
            comboBox.Items.Clear();
            comboBox.ItemsSource = groups;
            comboBox.SelectedIndex = 0;
        }

        private void RecipeGroupIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            //RecipeGroupIdTextBlock.Text = "GroupId: " + (comboBox.SelectedItem as Group).Id.ToString();    
            RecipeGroupIdTextBlock.Text = "GroupId: " + comboBox.SelectedValue.ToString();
        }

        private async void AddRecipeButton_Click(object sender, RoutedEventArgs e)
        {
            if (RecipeTitleTextBox.Text == "")
                MessageBox.Show("Field \"Title\" is empty!");
            else if (RecipeImageNameTextBox.Text == "")
                MessageBox.Show("Field \"Image Name\" is empty!");
            else if (RecipeImagePathTextBox.Text == "")
                MessageBox.Show("Field \"Image Path\" is empty!");
            else
            {
                RecipeGrid.IsEnabled = false;
                ProgressBar.IsIndeterminate = true;

                //var groupId = (RecipeGroupIdComboBox.SelectedItem as Group).Id;
                var groupId = Convert.ToInt16(RecipeGroupIdComboBox.SelectedValue);
                var title = RecipeTitleTextBox.Text;
                var imageName = RecipeImageNameTextBox.Text;
                var imagePath = RecipeImagePathTextBox.Text;

                Item itemDetail = new Item { GroupId = groupId, Title = title, ImagePath = imagePath, Content = "", Description = title };
                await MobileService.GetTable<Item>().InsertAsync(itemDetail);
                if (await LoadImageInBlob(imageName, imagePath))
                {
                    ProgressBar.IsIndeterminate = false;
                    MessageBox.Show("Recipe succesfully added");
                    RecipeGrid.IsEnabled = true;
                }
            }
        }

        public static async Task<bool> LoadImageInBlob(string pictureName, string picturePath)
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(constants.StorageConnectionString);
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
            CloudBlobContainer container = blobClient.GetContainerReference("images");
            CloudBlockBlob blockBlob = container.GetBlockBlobReference(pictureName);
            try
            {
                using (var fileStream = System.IO.File.OpenRead(picturePath))
                {
                    await Task.Factory.StartNew(() =>
                    {
                        blockBlob.UploadFromStream(fileStream);
                    });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
            return true;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.InitialDirectory = "c:\\";
            openFileDialog.Filter = "All files (*.*)|*.*|PNG (*.png)|*.png|JPG (*.jpg)|*.jpg|Bitmap (*.bmp)|*.bmp";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;

            var result = openFileDialog.ShowDialog();

            if (result == true)
            {
                RecipeImagePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            RecipeImageNameTextBox.Text = System.Guid.NewGuid().ToString();
        }

        private void RecipeImageNameTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressBar.IsIndeterminate = true;
            RecipeImageNameTextBox.Text = System.Guid.NewGuid().ToString();
            ProgressBar.IsIndeterminate = false;
        }

        private async void IngredientRecipeIdComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            ProgressBar.IsIndeterminate = true;
            List<Item> RecipeId = await MobileService.GetTable<Item>().ToListAsync();
            var comboBox = sender as ComboBox;
            comboBox.Items.Clear();
            comboBox.ItemsSource = RecipeId;
            comboBox.SelectedIndex = 0;
            IngredientGrid.IsEnabled = true;
            ProgressBar.IsIndeterminate = false;
        }

        private void IngredientRecipeIdComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            IngredientRecipeIdTextBlock.Text = "RecipeId: " + (comboBox.SelectedItem as Group).Id.ToString();
        }

        private async void AddIngredientButton_Click(object sender, RoutedEventArgs e)
        {
            if (IngredientNameTextBox.Text == "")
                MessageBox.Show("Field \"Name\" is empty!");
            else if (IngredientAmountTextBox.Text == "")
                MessageBox.Show("Field \"Amount\" is empty!");
            else
            {
                ProgressBar.IsIndeterminate = true;

                //var recipeId = (IngredientRecipeIdComboBox.SelectedItem as Item).Id;
                //var recipeName = (IngredientRecipeIdComboBox.SelectedItem as Item).Title;

                var recipeId = Convert.ToInt16(IngredientRecipeIdComboBox.SelectedValue);
                var recipeName = IngredientRecipeIdComboBox.SelectedValue.ToString();
                var name = IngredientNameTextBox.Text;
                var amount = Convert.ToInt16(IngredientAmountTextBox.Text);

                //var unit = (IngredientUnitComboBox.SelectedItem as Ingredient).Unit;
                //var aisle = (IngredientAisleComboBox.SelectedItem as Ingredient).Unit;

                var unit = IngredientUnitComboBox.SelectedValue.ToString();
                var aisle = IngredientAisleComboBox.SelectionBoxItem.ToString();

                Ingredient ingredientDetail = new Ingredient { RecipeId = recipeId, RecipeName = recipeName, Name = name, Amount = amount, Unit = unit, Aisle = aisle };
                await MobileService.GetTable<Ingredient>().InsertAsync(ingredientDetail);

                ProgressBar.IsIndeterminate = false;
                MessageBox.Show("Ingredient succesfully added");
            }
        }

        private void IngredientUnitComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            List<UnitsName> IngredientUnitsNames = new List<UnitsName>() 
            {
                new UnitsName { FullName = "Упаковки", ShortName = "уп" },
                new UnitsName { FullName = "Килограммы", ShortName = "кг" },
                new UnitsName { FullName = "Граммы", ShortName = "г" },
                new UnitsName { FullName = "Милилитры", ShortName = "мл" },
                new UnitsName { FullName = "Литры", ShortName = "л" },
                new UnitsName { FullName = "Банки", ShortName = "бан" },
                new UnitsName { FullName = "Штуки", ShortName = "шт" },
                new UnitsName { FullName = "Пучки", ShortName = "пуч" },
                new UnitsName { FullName = "Зубчики", ShortName = "зуб" },
            };

            var comboBox = sender as ComboBox;
            comboBox.ItemsSource = IngredientUnitsNames;
            comboBox.SelectedIndex = 0;
        }

        private void IngredientUnitComboBox_SelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            var comboBox = sender as ComboBox;
            IngredientUnitShortNameTextBlock.Text = "ShortName: " + comboBox.SelectedValue.ToString();
        }
    }

    public class UnitsName
    {
        public string FullName { get; set; }
        public string ShortName { get; set; }
    }
    
    public class Group
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImagePath { get; set; }
        public string Description { get; set; }

        public Group()
        {

        }
    }

    public class Item : Group
    {
        public int GroupId { get; set; }
        public string Content { get; set; }
        public Item()
        {

        }
    }

    public class ItemDetail
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public string MainPicture { get; set; }
        public string Description { get; set; }

    }

    public class Step
    {
        public int Id { get; set; }
        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string Description { get; set; }
        public string PictureName { get; set; }
        public string StepNumber { get; set; }
    }

    public class Person
    {
        public string FirstName { get; set; }
    }

    public class Ingredient
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Amount { get; set; }
        public string Unit { get; set; }
        public int RecipeId { get; set; }
        public string RecipeName { get; set; }
        public string Aisle { get; set; }
    }
}
