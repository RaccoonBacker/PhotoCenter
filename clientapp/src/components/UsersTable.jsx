import { useEffect, useMemo, useState } from 'react';
import {
  createUser,
  getUsers,
  updateUser,
} from '../api/api';
import './AdminTables.css';
import './UsersTable.css';

const PAGE_SIZE = 10;

const ROLE_OPTIONS = [
  { value: 'Client', label: 'Клиент' },
  { value: 'Employee', label: 'Сотрудник' },
  { value: 'Admin', label: 'Администратор' },
];

const STATUS_OPTIONS = [
  { value: true, label: 'Активен' },
  { value: false, label: 'Заблокирован' },
];

function getRoleLabel(role) {
  const item = ROLE_OPTIONS.find(
    (option) => option.value === role
  );

  return item ? item.label : role || '—';
}

function getName(user) {
  if (user.name) {
    return user.name;
  }

  const firstName = user.firstName || '';
  const lastName = user.lastName || '';

  return `${firstName} ${lastName}`.trim();
}

function createEmptyUser(id) {
  return {
    id,
    login: '',
    name: '',
    role: 'Client',
    isActive: true,
  };
}

function normalizeUser(user) {
  return {
    id: user.id ?? user.ID,
    login: user.login ?? user.Login ?? '',
    name:
      user.name ??
      `${user.firstName ?? user.FirstName ?? ''} ${
        user.lastName ?? user.LastName ?? ''
      }`.trim(),
    role: user.role ?? user.Role ?? 'Client',
    isActive:
      user.isActive ??
      user.IsActive ??
      true,
  };
}

function UsersTable({ onDataChanged }) {
  const [users, setUsers] = useState([]);
  const [newUser, setNewUser] = useState(null);
  const [changedUsers, setChangedUsers] = useState({});
  const [currentPage, setCurrentPage] = useState(1);

  const [isLoading, setIsLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);

  const [error, setError] = useState('');
  const [success, setSuccess] = useState('');

  const loadData = async () => {
    setIsLoading(true);
    setError('');
    setSuccess('');

    try {
      const data = await getUsers();

      const normalized = (data || []).map(
        normalizeUser
      );

      setUsers(normalized);
      setChangedUsers({});

      const maxId = normalized.reduce(
        (max, user) =>
          Math.max(max, Number(user.id) || 0),
        0
      );

      setNewUser(createEmptyUser(maxId + 1));

      const totalPages = Math.max(
        1,
        Math.ceil(
          normalized.length / PAGE_SIZE
        )
      );

      setCurrentPage((page) =>
        Math.min(page, totalPages)
      );
    } catch (requestError) {
      setError(
        requestError.message ||
        'Не удалось загрузить пользователей.'
      );
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const updateExistingUser = (
    id,
    field,
    value
  ) => {
    setUsers((current) =>
      current.map((user) =>
        user.id === id
          ? {
              ...user,
              [field]: value,
            }
          : user
      )
    );

    setChangedUsers((current) => ({
      ...current,
      [id]: {
        ...(current[id] || {}),
        [field]: value,
      },
    }));
  };

  const updateNewUser = (
    field,
    value
  ) => {
    setNewUser((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const validateUser = (user) => {
    if (!user.login.trim()) {
      return 'Логин обязателен.';
    }

    if (!user.name.trim()) {
      return 'Имя и фамилия обязательны.';
    }

    const nameParts = user.name
      .trim()
      .split(/\s+/);

    if (nameParts.length < 2) {
      return 'Укажите имя и фамилию.';
    }

    if (!user.role) {
      return 'Роль обязательна.';
    }

    return '';
  };

  const handleSave = async () => {
    setError('');
    setSuccess('');

    if (newUser) {
      const hasNewData =
        newUser.login.trim() ||
        newUser.name.trim();

      if (hasNewData) {
        const validationError =
          validateUser(newUser);

        if (validationError) {
          setError(validationError);
          return;
        }
      }
    }

    for (const user of users) {
      const changes = changedUsers[user.id];

      if (!changes) {
        continue;
      }

      const mergedUser = {
        ...user,
        ...changes,
      };

      const validationError =
        validateUser(mergedUser);

      if (validationError) {
        setError(
          `Пользователь ID ${user.id}: ${validationError}`
        );
        return;
      }
    }

    setIsSaving(true);

    try {
      const updateRequests =
        Object.entries(changedUsers).map(
          ([id, data]) => {
            const user = users.find(
              (item) =>
                Number(item.id) === Number(id)
            );

            return updateUser(
              Number(id),
              {
                Login:
                  data.login ??
                  user.login,
                Name:
                  data.name ??
                  user.name,
                Role:
                  data.role ??
                  user.role,
                IsActive:
                  data.isActive ??
                  user.isActive,
              }
            );
          }
        );

      if (
        newUser &&
        newUser.login.trim() &&
        newUser.name.trim()
      ) {
        updateRequests.push(
          createUser({
            Login: newUser.login.trim(),
            Name: newUser.name.trim(),
            Role: newUser.role,
            IsActive: newUser.isActive,
          })
        );
      }

      await Promise.all(updateRequests);

      await loadData();

      if (onDataChanged) {
        await onDataChanged();
      }

      setSuccess(
        'Изменения пользователей сохранены.'
      );
    } catch (requestError) {
      setError(
        requestError.message ||
        'Не удалось сохранить изменения.'
      );
    } finally {
      setIsSaving(false);
    }
  };

  const clearNewUser = () => {
    if (!newUser) {
      return;
    }

    setNewUser(
      createEmptyUser(newUser.id)
    );
    setError('');
  };

  const totalPages = Math.max(
    1,
    Math.ceil(users.length / PAGE_SIZE)
  );

  const visibleUsers = useMemo(() => {
    const start =
      (currentPage - 1) * PAGE_SIZE;

    return users.slice(
      start,
      start + PAGE_SIZE
    );
  }, [users, currentPage]);

  if (isLoading) {
    return (
      <section className="admin-card">
        <div className="admin-empty">
          Загрузка пользователей...
        </div>
      </section>
    );
  }

  return (
    <section className="admin-card">
      <div className="admin-card-header">
        <div>
          <h2>Пользователи</h2>
          <p>
            Редактирование пользователей системы.
          </p>
        </div>

        <span className="admin-counter">
          {users.length}
        </span>
      </div>

      {error && (
        <div className="admin-message admin-message-error">
          {error}
        </div>
      )}

      {success && (
        <div className="admin-message admin-message-success">
          {success}
        </div>
      )}

      <div className="admin-table-wrap">
        <table className="admin-table admin-users-table">
          <thead>
            <tr>
              <th>ID</th>
              <th>Логин</th>
              <th>Имя</th>
              <th>Роль</th>
              <th>Статус</th>
            </tr>
          </thead>

          <tbody>
            {visibleUsers.map((user) => (
              <tr key={user.id}>
                <td>{user.id}</td>

                <td>
                  <input
                    type="text"
                    className="admin-table-input"
                    value={user.login}
                    onChange={(event) =>
                      updateExistingUser(
                        user.id,
                        'login',
                        event.target.value
                      )
                    }
                  />
                </td>

                <td>
                  <input
                    type="text"
                    className="admin-table-input"
                    value={getName(user)}
                    placeholder="Имя Фамилия"
                    onChange={(event) =>
                      updateExistingUser(
                        user.id,
                        'name',
                        event.target.value
                      )
                    }
                  />
                </td>

                <td>
                  <select
                    className="admin-table-select"
                    value={user.role}
                    onChange={(event) =>
                      updateExistingUser(
                        user.id,
                        'role',
                        event.target.value
                      )
                    }
                  >
                    {ROLE_OPTIONS.map(
                      (option) => (
                        <option
                          key={option.value}
                          value={option.value}
                        >
                          {option.label}
                        </option>
                      )
                    )}
                  </select>
                </td>

                <td>
                  <select
                    className="admin-table-select"
                    value={
                      user.isActive
                        ? 'true'
                        : 'false'
                    }
                    onChange={(event) =>
                      updateExistingUser(
                        user.id,
                        'isActive',
                        event.target.value ===
                          'true'
                      )
                    }
                  >
                    {STATUS_OPTIONS.map(
                      (option) => (
                        <option
                          key={
                            String(
                              option.value
                            )
                          }
                          value={String(
                            option.value
                          )}
                        >
                          {option.label}
                        </option>
                      )
                    )}
                  </select>
                </td>
              </tr>
            ))}

            {newUser && (
              <tr className="admin-new-row">
                <td>{newUser.id}</td>

                <td>
                  <input
                    type="text"
                    className="admin-table-input"
                    value={newUser.login}
                    placeholder="Новый логин"
                    onChange={(event) =>
                      updateNewUser(
                        'login',
                        event.target.value
                      )
                    }
                  />
                </td>

                <td>
                  <input
                    type="text"
                    className="admin-table-input"
                    value={newUser.name}
                    placeholder="Имя Фамилия *"
                    onChange={(event) =>
                      updateNewUser(
                        'name',
                        event.target.value
                      )
                    }
                  />
                </td>

                <td>
                  <select
                    className="admin-table-select"
                    value={newUser.role}
                    onChange={(event) =>
                      updateNewUser(
                        'role',
                        event.target.value
                      )
                    }
                  >
                    {ROLE_OPTIONS.map(
                      (option) => (
                        <option
                          key={option.value}
                          value={option.value}
                        >
                          {option.label}
                        </option>
                      )
                    )}
                  </select>
                </td>

                <td>
                  <select
                    className="admin-table-select"
                    value={
                      newUser.isActive
                        ? 'true'
                        : 'false'
                    }
                    onChange={(event) =>
                      updateNewUser(
                        'isActive',
                        event.target.value ===
                          'true'
                      )
                    }
                  >
                    {STATUS_OPTIONS.map(
                      (option) => (
                        <option
                          key={
                            String(
                              option.value
                            )
                          }
                          value={String(
                            option.value
                          )}
                        >
                          {option.label}
                        </option>
                      )
                    )}
                  </select>
                </td>
              </tr>
            )}
          </tbody>
        </table>
      </div>

      <div className="admin-table-actions">
        <button
          type="button"
          className="admin-secondary-button"
          onClick={clearNewUser}
        >
          Очистить новую строку
        </button>

        <button
          type="button"
          className="admin-primary-button"
          onClick={handleSave}
          disabled={isSaving}
        >
          {isSaving
            ? 'Сохранение...'
            : 'Сохранить изменения'}
        </button>
      </div>

      {totalPages > 1 && (
        <div className="admin-pagination">
          {Array.from(
            { length: totalPages },
            (_, index) => index + 1
          ).map((page) => (
            <button
              key={page}
              type="button"
              className={
                page === currentPage
                  ? 'admin-page-button admin-page-button-active'
                  : 'admin-page-button'
              }
              onClick={() =>
                setCurrentPage(page)
              }
            >
              {page}
            </button>
          ))}
        </div>
      )}
    </section>
  );
}

export default UsersTable;