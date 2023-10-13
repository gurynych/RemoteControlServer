namespace RemoteControlServer.BusinessLogic.Repository
{
    public interface IGenericRepository<T> 
        where T : class
    {
        /// <summary>
        /// Получение всех объектов
        /// </summary>
        /// <returns></returns>
        IEnumerable<T> GetAll();

        /// <summary>
        /// Получение объекта по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        T Get(int id);

        /// <summary>
        /// Добавление объекта
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true - объект добавлен, false - ошибка добавления</returns>
        bool AddItem(T item);

        /// <summary>
        /// Изменение объекта
        /// </summary>
        /// <param name="item"></param>
        /// <returns>true - объект изменен, false - ошибка обновления</returns>
        bool Update(T item);

        /// <summary>
        /// Удаление объекта по id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>true - объект удален, false - такого объекта нет</returns>
        bool Delete(int id);

        /// <summary>
        /// Сохранение изменений
        /// </summary>
        /// <returns>true - объект сохранен, false - ошибка сохранения</returns>
        bool SaveChanges();
    }
}
