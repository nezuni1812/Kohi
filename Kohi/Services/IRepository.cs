using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Services
{
    public interface IRepository<T>
    {
        List<T> GetAll(
            int pageNumber = 1,              // Số trang (mặc định là 1)
            int pageSize = 10,               // Số bản ghi trên mỗi trang (mặc định là 10)
            string sortBy = null,            // Trường để sắp xếp (ví dụ: "Name", "CreatedDate")
            bool sortDescending = false,     // Sắp xếp giảm dần hay tăng dần (mặc định tăng dần)
            string filterField = null,       // Trường để lọc (ví dụ: "Status")
            string filterValue = null,       // Giá trị để lọc (ví dụ: "Active")
            string searchKeyword = null      // Từ khóa tìm kiếm
        );

        T GetById(string id);
        int DeleteById(string id);
        int UpdateById(string id, T info);
        int Insert(string id, T info);
        int GetCount(string filterField = null, string filterValue = null, string searchKeyword = null);
    }
}
