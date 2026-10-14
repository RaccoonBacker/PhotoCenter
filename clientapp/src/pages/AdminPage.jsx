import { useEffect, useState } from 'react';
import Tabs from '../components/Tabs';
import UsersTable from '../components/UsersTable';
import OrdersTable from '../components/OrdersTable';
import Analytics from '../components/Analytics';
import DeletePanel from '../components/DeletePanel';
import ReportModal from '../components/ReportModal';
import {
  clearAuth,
  deleteOrder,
  deleteUser,
  getAnalyticsRevenue,
  getAnalyticsStatuses,
  getAnalyticsTopServices,
  getCurrentUser,
  getOrders,
  getUsers,
} from '../api/api';
import './AdminPage.css';

const tabs = [
  { id: 'users', label: 'Пользователи' },
  { id: 'orders', label: 'Заказы' },
  { id: 'analytics', label: 'Аналитика' },
];

function getRoleLabel(role) {
  const roles = {
    Admin: 'Администратор',
    Client: 'Клиент',
    Employee: 'Сотрудник',
  };

  return roles[role] || role || '—';
}

function getOrderStatusLabel(status) {
  const statuses = {
    New: 'Новый',
    InProgress: 'В работе',
    Ready: 'Готов',
    Completed: 'Выдан',
    Cancelled: 'Отменён',
  };

  return statuses[status] || status || '—';
}

function formatDate(value) {
  if (!value) {
    return '—';
  }

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return date.toLocaleDateString('ru-RU');
}

function AdminPage() {
  const [activeTab, setActiveTab] = useState('users');
  const [isReportOpen, setIsReportOpen] = useState(false);

  const [users, setUsers] = useState([]);
  const [orders, setOrders] = useState([]);

  const [analytics, setAnalytics] = useState({
    statuses: [],
    revenue: [],
    topServices: [],
  });

  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState('');

  const currentUser = getCurrentUser();

  const loadData = async () => {
    setIsLoading(true);
    setError('');

    try {
      const usersData = await getUsers();
      setUsers(usersData || []);

      const ordersData = await getOrders();
      setOrders(ordersData || []);

      const statusesData = await getAnalyticsStatuses();
      const revenueData = await getAnalyticsRevenue();
      const topServicesData = await getAnalyticsTopServices();

      setAnalytics({
        statuses: statusesData || [],
        revenue: revenueData || [],
        topServices: topServicesData || [],
      });
    } catch (requestError) {
      setError(
        requestError.message ||
        'Не удалось загрузить данные панели администратора.'
      );

      if (!getCurrentUser()) {
        clearAuth();
      }
    } finally {
      setIsLoading(false);
    }
  };

  useEffect(() => {
    if (
      !currentUser ||
      currentUser.role !== 'Admin'
    ) {
      setIsLoading(false);
      return;
    }

    loadData();
  }, []);

  if (!currentUser) {
    return (
      <div className="admin-page">
        <section className="admin-card">
          <div className="admin-card-header">
            <div>
              <h2>Требуется авторизация</h2>
              <p>
                Войдите в систему с учётной записью
                администратора.
              </p>
            </div>
          </div>
        </section>
      </div>
    );
  }

  if (currentUser.role !== 'Admin') {
    return (
      <div className="admin-page">
        <section className="admin-card">
          <div className="admin-card-header">
            <div>
              <h2>Доступ запрещён</h2>
              <p>
                Панель администратора доступна только
                пользователям с ролью администратора.
              </p>
            </div>
          </div>
        </section>
      </div>
    );
  }

  const handleDelete = async ({ entity, id }) => {
    const numericId = Number(id);

    if (!Number.isInteger(numericId) || numericId < 1) {
      window.alert('Укажите корректный ID.');
      return;
    }

    const confirmed = window.confirm(
      `Удалить ${entity === 'user' ? 'пользователя' : 'заказ'} с ID ${numericId}?`
    );

    if (!confirmed) {
      return;
    }

    try {
      if (entity === 'user') {
        await deleteUser(numericId);
      } else {
        await deleteOrder(numericId);
      }

      await loadData();
    } catch (requestError) {
      window.alert(
        requestError.message ||
        'Не удалось удалить запись.'
      );
    }
  };


  const totalOrders = analytics.statuses.reduce(
    (sum, item) =>
      sum + Number(item.Count || 0),
    0
  );

  const totalRevenue = analytics.revenue.reduce(
    (sum, item) =>
      sum + Number(item.Revenue || 0),
    0
  );

  const statusRows = analytics.statuses.map(
    (item) => ({
      label: getOrderStatusLabel(item.Status),
      value: Number(item.Count || 0),
    })
  );

  return (
    <div className="admin-page">
      <section className="admin-hero">
        <div>
          <span className="admin-eyebrow">
            Фотоцентр
          </span>

          <h1>Панель администратора</h1>

          <p>
            Управление пользователями, заказами и
            аналитикой системы.
          </p>
        </div>

        <button
          type="button"
          className="admin-report-button"
          onClick={() => setIsReportOpen(true)}
        >
          Отчёт
        </button>
      </section>

      <Tabs
        tabs={tabs}
        activeTab={activeTab}
        onChange={setActiveTab}
      />

      <div className="admin-content">
        {isLoading && (
          <section className="admin-card">
            <div className="admin-empty">
              Загрузка данных...
            </div>
          </section>
        )}

        {!isLoading && error && (
          <section className="admin-card">
            <div className="admin-empty">
              {error}
            </div>
          </section>
        )}

        {!isLoading && !error && (
          <>
            {activeTab === 'users' && (
              <UsersTable
                onDataChanged={loadData}
              />
            )}

            {activeTab === 'orders' && (
              <OrdersTable
                onDataChanged={loadData}
              />
            )}

            {activeTab === 'analytics' && (
              <Analytics
                stats={{
                  users: users.length,
                  orders: totalOrders,
                  revenue: `${totalRevenue.toFixed(2)} руб.`,
                  statuses: statusRows,
                  revenueData: analytics.revenue,
                  topServices: analytics.topServices,
                }}
              />
            )}

            <DeletePanel
              onDelete={handleDelete}
            />
          </>
        )}
      </div>

      <ReportModal
        isOpen={isReportOpen}
        onClose={() => setIsReportOpen(false)}
        users={users}
        orders={orders}
        analytics={analytics}
      />
    </div>
  );
}

export default AdminPage;
