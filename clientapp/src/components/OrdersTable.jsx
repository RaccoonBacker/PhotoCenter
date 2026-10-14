import { useEffect, useMemo, useState } from 'react';
import {
  createOrder,
  getOrders,
  getUsers,
  updateOrder,
} from '../api/api';

import './OrdersTable.css';

const ORDERS_PER_PAGE = 10;

const STATUS_OPTIONS = [
  {
    value: 'New',
    label: 'Новый',
  },
  {
    value: 'InProgress',
    label: 'В работе',
  },
  {
    value: 'Ready',
    label: 'Готов',
  },
  {
    value: 'Completed',
    label: 'Выдан',
  },
  {
    value: 'Cancelled',
    label: 'Отменён',
  },
];

function formatDateForInput(value) {
  if (!value) return '';

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return '';
  }

  const year = date.getFullYear();

  const month = String(
    date.getMonth() + 1
  ).padStart(2, '0');

  const day = String(
    date.getDate()
  ).padStart(2, '0');

  const hours = String(
    date.getHours()
  ).padStart(2, '0');

  const minutes = String(
    date.getMinutes()
  ).padStart(2, '0');

  return `${year}-${month}-${day}T${hours}:${minutes}`;
}

function formatDateForDisplay(value) {
  if (!value) return '—';

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleString(
    'ru-RU',
    {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }
  );
}

function getPersonName(person) {
  if (!person) {
    return '—';
  }

  if (person.name) {
    return person.name;
  }

  return [
    person.lastName,
    person.firstName,
  ]
    .filter(Boolean)
    .join(' ') || '—';
}

function OrdersTable({ onDataChanged }) {
  const [orders, setOrders] =
    useState([]);

  const [clients, setClients] =
    useState([]);

  const [employees, setEmployees] =
    useState([]);

  const [newOrder, setNewOrder] =
    useState({
      clientId: '',
      employeeId: '',
      createdDate:
        formatDateForInput(
          new Date()
        ),
      totalPrice: '',
      status: 'New',
    });

  const [changedOrders, setChangedOrders] =
    useState({});

  const [currentPage, setCurrentPage] =
    useState(1);

  const [isLoading, setIsLoading] =
    useState(true);

  const [isSaving, setIsSaving] =
    useState(false);

  const [error, setError] =
    useState('');

  const [success, setSuccess] =
    useState('');

  const loadData = async () => {
    setIsLoading(true);
    setError('');

    try {
      const ordersData =
        await getOrders();

      const usersData =
        await getUsers();

      setOrders(
        Array.isArray(ordersData)
          ? ordersData
          : []
      );

      const allUsers =
        Array.isArray(usersData)
          ? usersData
          : [];

      const clientUsers =
        allUsers.filter(
          user =>
            user.role === 'Client' &&
            Number(user.clientId) > 0
        );

      const employeeUsers =
        allUsers.filter(
          user =>
            user.role === 'Employee' &&
            Number(user.employeeId) > 0
        );

      setClients(clientUsers);
      setEmployees(employeeUsers);

      setChangedOrders({});
    } catch (requestError) {
      setError(
        requestError.message ||
        'Не удалось загрузить заказы.'
      );
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const totalPages = Math.max(
    1,
    Math.ceil(
      orders.length /
      ORDERS_PER_PAGE
    )
  );

  useEffect(() => {
    if (
      currentPage >
      totalPages
    ) {
      setCurrentPage(
        totalPages
      );
    }
  }, [
    currentPage,
    totalPages,
  ]);

  const visibleOrders =
    useMemo(() => {
      const start =
        (currentPage - 1) *
        ORDERS_PER_PAGE;

      return orders.slice(
        start,
        start + ORDERS_PER_PAGE
      );
    }, [
      orders,
      currentPage,
    ]);

  const nextOrderId =
    orders.length > 0
      ? Math.max(
          ...orders.map(
            order =>
              Number(order.id)
          )
        ) + 1
      : 1;

  const selectedNewClient =
    clients.find(
      client =>
        String(client.clientId) ===
        String(newOrder.clientId)
    );

  const selectedNewEmployee =
    employees.find(
      employee =>
        String(employee.employeeId) ===
        String(newOrder.employeeId)
    );

  const markOrderChanged = (
    id,
    field,
    value
  ) => {
    setOrders(
      previous =>
        previous.map(order =>
          order.id === id
            ? {
                ...order,
                [field]: value,
              }
            : order
        )
    );

    setChangedOrders(
      previous => ({
        ...previous,
        [id]: {
          ...(previous[id] || {}),
          [field]: value,
        },
      })
    );

    setError('');
    setSuccess('');
  };

  const handleNewOrderChange = (
    field,
    value
  ) => {
    setNewOrder(
      previous => ({
        ...previous,
        [field]: value,
      })
    );

    setError('');
    setSuccess('');
  };

  const handleSave = async () => {
    setError('');
    setSuccess('');

    const hasNewOrder =
      Boolean(
        newOrder.clientId
      ) ||
      Boolean(
        newOrder.employeeId
      ) ||
      Boolean(
        newOrder.totalPrice
      );

    if (
      !hasNewOrder &&
      Object.keys(
        changedOrders
      ).length === 0
    ) {
      setError(
        'Нет изменений для сохранения.'
      );
      return;
    }

    if (
      hasNewOrder &&
      !newOrder.clientId
    ) {
      setError(
        'Для нового заказа необходимо выбрать клиента.'
      );
      return;
    }

    if (
      hasNewOrder &&
      !newOrder.employeeId
    ) {
      setError(
        'Для нового заказа необходимо выбрать исполнителя.'
      );
      return;
    }

    if (
      hasNewOrder &&
      newOrder.totalPrice === ''
    ) {
      setError(
        'Для нового заказа необходимо указать сумму.'
      );
      return;
    }

    setIsSaving(true);

    try {
      for (
        const [id, data]
        of Object.entries(
          changedOrders
        )
      ) {
        await updateOrder(
          Number(id),
          {
            ClientID:
              data.clientId !== undefined
                ? Number(data.clientId)
                : undefined,

            EmployeeID:
              data.employeeId !== undefined
                ? Number(data.employeeId)
                : undefined,

            CreatedDate:
              data.createdDate,

            TotalPrice:
              data.totalPrice !== undefined
                ? Number(data.totalPrice)
                : undefined,

            Status:
              data.status,
          }
        );
      }

      if (hasNewOrder) {
        const parsedPrice =
          Number(
            newOrder.totalPrice
          );

        if (
          Number.isNaN(
            parsedPrice
          ) ||
          parsedPrice < 0
        ) {
          throw new Error(
            'Сумма нового заказа указана некорректно.'
          );
        }

        await createOrder({
          ClientID:
            Number(
              newOrder.clientId
            ),

          EmployeeID:
            Number(
              newOrder.employeeId
            ),

          CreatedDate:
            new Date(
              newOrder.createdDate
            ).toISOString(),

          TotalPrice:
            parsedPrice,

          Status:
            newOrder.status,
        });
      }

      setNewOrder({
        clientId: '',
        employeeId: '',
        createdDate:
          formatDateForInput(
            new Date()
          ),
        totalPrice: '',
        status: 'New',
      });

      setChangedOrders({});

      await loadData();

      if (onDataChanged) {
        await onDataChanged();
      }

      setSuccess(
        'Данные заказов успешно обновлены.'
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

  const renderPagination = () => {
    if (totalPages <= 1) {
      return null;
    }

    return (
      <div className="admin-pagination">
        <button
          type="button"
          className="admin-page-button"
          disabled={
            currentPage === 1
          }
          onClick={() =>
            setCurrentPage(
              page =>
                Math.max(
                  1,
                  page - 1
                )
            )
          }
        >
          ‹
        </button>

        {Array.from(
          {
            length: totalPages,
          },
          (_, index) => {
            const page =
              index + 1;

            return (
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
            );
          }
        )}

        <button
          type="button"
          className="admin-page-button"
          disabled={
            currentPage ===
            totalPages
          }
          onClick={() =>
            setCurrentPage(
              page =>
                Math.min(
                  totalPages,
                  page + 1
                )
            )
          }
        >
          ›
        </button>
      </div>
    );
  };

  return (
    <section className="admin-card">
      <div className="admin-card-header">
        <div>
          <h2>Заказы</h2>

          <p>
            Текущие заказы фотоцентра.
          </p>
        </div>

        <span className="admin-counter">
          {orders.length}
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

      {renderPagination()}

      <div className="admin-table-wrap">
        <table className="admin-table admin-orders-table">
          <thead>
            <tr>
              <th>№</th>
              <th>Клиент</th>
              <th>Логин</th>
              <th>Исполнитель</th>
              <th>Дата</th>
              <th>Сумма</th>
              <th>Статус</th>
            </tr>
          </thead>

          <tbody>
            {isLoading ? (
              <tr>
                <td
                  colSpan="7"
                  className="admin-empty"
                >
                  Загрузка...
                </td>
              </tr>
            ) : visibleOrders.length === 0 ? (
              <tr>
                <td
                  colSpan="7"
                  className="admin-empty"
                >
                  Данных пока нет
                </td>
              </tr>
            ) : (
              visibleOrders.map(
                order => (
                  <tr key={order.id}>
                    <td>
                      #{order.id}
                    </td>

                    <td>
                      {order.clientName ||
                        '—'}
                    </td>

                    <td>
                      <span className="admin-login">
                        {order.clientLogin ||
                          '—'}
                      </span>
                    </td>

                    <td>
                      <select
                        className="admin-table-select"
                        value={
                          order.employeeId ??
                          ''
                        }
                        onChange={event =>
                          markOrderChanged(
                            order.id,
                            'employeeId',
                            event.target.value
                          )
                        }
                      >
                        <option value="">
                          Выберите исполнителя
                        </option>

                        {employees.map(
                          employee => (
                            <option
                              key={
                                employee.employeeId
                              }
                              value={
                                employee.employeeId
                              }
                            >
                              {getPersonName(
                                employee
                              )}
                            </option>
                          )
                        )}
                      </select>
                    </td>

                    <td>
                      <input
                        type="datetime-local"
                        className="admin-table-input"
                        value={
                          formatDateForInput(
                            order.createdDate
                          )
                        }
                        onChange={event =>
                          markOrderChanged(
                            order.id,
                            'createdDate',
                            event.target.value
                          )
                        }
                      />

                      <small className="admin-date-preview">
                        {formatDateForDisplay(
                          order.createdDate
                        )}
                      </small>
                    </td>

                    <td>
                      <input
                        type="number"
                        min="0"
                        step="0.01"
                        className="admin-table-input admin-price-input"
                        value={
                          order.totalPrice ??
                          ''
                        }
                        onChange={event =>
                          markOrderChanged(
                            order.id,
                            'totalPrice',
                            event.target.value
                          )
                        }
                      />
                    </td>

                    <td>
                      <select
                        className="admin-table-select"
                        value={
                          order.status ||
                          'New'
                        }
                        onChange={event =>
                          markOrderChanged(
                            order.id,
                            'status',
                            event.target.value
                          )
                        }
                      >
                        {STATUS_OPTIONS.map(
                          option => (
                            <option
                              key={
                                option.value
                              }
                              value={
                                option.value
                              }
                            >
                              {option.label}
                            </option>
                          )
                        )}
                      </select>
                    </td>
                  </tr>
                )
              )
            )}

            {!isLoading && (
              <tr className="admin-new-row">
                <td>
                  #{nextOrderId}
                </td>

                <td>
                  <select
                    className="admin-table-select"
                    value={
                      newOrder.clientId
                    }
                    onChange={event =>
                      handleNewOrderChange(
                        'clientId',
                        event.target.value
                      )
                    }
                  >
                    <option value="">
                      Выберите клиента
                    </option>

                    {clients.map(
                      client => (
                        <option
                          key={
                            client.clientId
                          }
                          value={
                            client.clientId
                          }
                        >
                          {getPersonName(
                            client
                          )}
                        </option>
                      )
                    )}
                  </select>
                </td>

                <td>
                  <span className="admin-login">
                    {selectedNewClient
                      ? selectedNewClient.login
                      : '—'}
                  </span>
                </td>

                <td>
                  <select
                    className="admin-table-select"
                    value={
                      newOrder.employeeId
                    }
                    onChange={event =>
                      handleNewOrderChange(
                        'employeeId',
                        event.target.value
                      )
                    }
                  >
                    <option value="">
                      Выберите исполнителя
                    </option>

                    {employees.map(
                      employee => (
                        <option
                          key={
                            employee.employeeId
                          }
                          value={
                            employee.employeeId
                          }
                        >
                          {getPersonName(
                            employee
                          )}
                        </option>
                      )
                    )}
                  </select>

                  {selectedNewEmployee && (
                    <small className="admin-date-preview">
                      Исполнитель выбран
                    </small>
                  )}
                </td>

                <td>
                  <input
                    type="datetime-local"
                    className="admin-table-input"
                    value={
                      newOrder.createdDate
                    }
                    onChange={event =>
                      handleNewOrderChange(
                        'createdDate',
                        event.target.value
                      )
                    }
                  />
                </td>

                <td>
                  <input
                    type="number"
                    min="0"
                    step="0.01"
                    className="admin-table-input admin-price-input"
                    placeholder="0.00"
                    value={
                      newOrder.totalPrice
                    }
                    onChange={event =>
                      handleNewOrderChange(
                        'totalPrice',
                        event.target.value
                      )
                    }
                  />
                </td>

                <td>
                  <select
                    className="admin-table-select"
                    value={
                      newOrder.status
                    }
                    onChange={event =>
                      handleNewOrderChange(
                        'status',
                        event.target.value
                      )
                    }
                  >
                    {STATUS_OPTIONS.map(
                      option => (
                        <option
                          key={
                            option.value
                          }
                          value={
                            option.value
                          }
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
          disabled={
            isSaving ||
            (
              !newOrder.clientId &&
              !newOrder.employeeId &&
              !newOrder.totalPrice
            )
          }
          onClick={() =>
            setNewOrder({
              clientId: '',
              employeeId: '',
              createdDate:
                formatDateForInput(
                  new Date()
                ),
              totalPrice: '',
              status: 'New',
            })
          }
        >
          Очистить новую строку
        </button>

        <button
          type="button"
          className="admin-primary-button"
          disabled={isSaving}
          onClick={handleSave}
        >
          {isSaving
            ? 'Сохранение...'
            : 'Обновить данные'}
        </button>
      </div>
    </section>
  );
}

export default OrdersTable;
