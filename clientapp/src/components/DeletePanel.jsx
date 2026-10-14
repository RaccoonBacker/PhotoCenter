import { useState } from 'react';
import './DeletePanel.css';

function DeletePanel({ onDelete }) {
  const [entity, setEntity] = useState('user');
  const [id, setId] = useState('');

  const handleSubmit = (event) => {
    event.preventDefault();
    if (!id.trim()) return;
    onDelete?.({ entity, id: id.trim() });
    setId('');
  };

  return (
    <section className="admin-card delete-panel">
      <div className="admin-card-header">
        <div>
          <h2>Удаление</h2>
          <p>Выберите тип записи и укажите её идентификатор.</p>
        </div>
      </div>

      <form className="delete-form" onSubmit={handleSubmit}>
        <label>
          Тип записи
          <select value={entity} onChange={(event) => setEntity(event.target.value)}>
            <option value="user">Пользователь</option>
            <option value="order">Заказ</option>
          </select>
        </label>

        <label>
          ID записи
          <input
            type="number"
            min="1"
            value={id}
            onChange={(event) => setId(event.target.value)}
            placeholder="Например, 12"
          />
        </label>

        <button type="submit" className="delete-button">
          Удалить
        </button>
      </form>
    </section>
  );
}

export default DeletePanel;
