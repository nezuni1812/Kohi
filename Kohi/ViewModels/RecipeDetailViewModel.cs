using Kohi.Models;
using Kohi.Services;
using Kohi.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.ViewModels
{
    public class RecipeDetailViewModel
    {
        private IDao _dao;
        public FullObservableCollection<RecipeDetailModel> RecipeDetails { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize); // Tổng số trang
        public RecipeDetailViewModel()
        {
            _dao = Service.GetKeyedSingleton<IDao>();
            RecipeDetails = new FullObservableCollection<RecipeDetailModel>();

            LoadData();
        }

        public async Task LoadData(int page = 1)
        {
            CurrentPage = page;
            TotalItems = _dao.RecipeDetails.GetCount(); // Lấy tổng số khách hàng từ DAO
            var result = await Task.Run(() => _dao.RecipeDetails.GetAll(
                pageNumber: CurrentPage,
                pageSize: PageSize
            )); // Lấy danh sách khách hàng phân trang
            RecipeDetails.Clear();
            foreach (var item in result)
            {
                RecipeDetails.Add(item);
            }
        }

        // Phương thức để chuyển đến trang tiếp theo
        public async Task NextPage()
        {
            if (CurrentPage < TotalPages)
            {
                await LoadData(CurrentPage + 1);
            }
        }

        // Phương thức để quay lại trang trước
        public async Task PreviousPage()
        {
            if (CurrentPage > 1)
            {
                await LoadData(CurrentPage - 1);
            }
        }

        // Phương thức để chuyển đến trang cụ thể
        public async Task GoToPage(int page)
        {
            if (page >= 1 && page <= TotalPages)
            {
                await LoadData(page);
            }
        }
        public async Task<int> Add(RecipeDetailModel recipeDetail)
        {
            try
            {
                int newId = _dao.RecipeDetails.Insert(recipeDetail);
                recipeDetail.Id = newId; 
                await LoadData(CurrentPage);
                return newId;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi thêm RecipeDetail: ProductVariantId = {recipeDetail.ProductVariantId}, Unit = {recipeDetail.Unit}, Error = {ex.Message}");
                throw; 
            }
        }

        public async Task<int> Delete(string id)
        {
            try
            {
                int result = _dao.RecipeDetails.DeleteById(id);
                await LoadData(CurrentPage);
                return result;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        public async Task Update(string id, RecipeDetailModel recipeDetail)
        {
            try
            {
                int result = _dao.RecipeDetails.UpdateById(id, recipeDetail);
            }
            catch (Exception ex)
            {

            }
        }

        public async Task<RecipeDetailModel> GetById(string id)
        {
            try
            {
                var result = await Task.Run(() => _dao.RecipeDetails.GetById(id));
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<List<RecipeDetailModel>> GetByProductVariantId(int productVariantId)
        {
            var recipeDetails = _dao.RecipeDetails.GetAll(1, int.MaxValue)
                .Where(rd => rd.ProductVariantId == productVariantId)
                .ToList();
            foreach (var recipe in recipeDetails)
            {
                recipe.Ingredient = _dao.Ingredients.GetById(recipe.IngredientId.ToString());
                if (recipe.Ingredient == null)
                {
                    Debug.WriteLine($"Warning: Không tìm thấy Ingredient với IngredientId = {recipe.IngredientId}");
                }
            }
            return recipeDetails;
        }
        public async Task<List<RecipeDetailModel>> GetAll()
        {
            try
            {
                var recipeDetails = _dao.RecipeDetails.GetAll(1, 1000); // Đồng bộ
                return recipeDetails;
            }
            catch (Exception ex)
            {
                return null; // Trả về null khi có lỗi
                // Xử lý lỗi (tùy chọn)
            }
        }
    }
}
