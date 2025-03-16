using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kohi.Services
{
    public interface IRepository<T>
    {
        List<T> GetAll(); //Phân trang, sắp xếp, lọc, tìm kiếm

        //T GetById(string id);
        //int DeleteById(string id);
        //int UpdateById(string id, T info);
        //int Insert(string id, T info);
    }
}
