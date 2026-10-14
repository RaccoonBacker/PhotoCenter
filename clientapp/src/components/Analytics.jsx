import './Analytics.css';

function Analytics({ stats }) {
  const items = stats?.statuses || [];

  const totalOrders = items.reduce(
    (sum, item) => sum + Number(item.value || 0),
    0
  );

  return (
    <section className="admin-card">
      <div className="admin-card-header">
        <div>
          <h2>Аналитика</h2>
          <p>
            Обзор основных показателей фотоцентра.
          </p>
        </div>
      </div>

      <div className="analytics-summary">
        <div className="analytics-metric">
          <span>Заказов</span>
          <strong>{stats?.orders ?? '—'}</strong>
        </div>

        <div className="analytics-metric">
          <span>Выручка</span>
          <strong>{stats?.revenue ?? '—'}</strong>
        </div>

        <div className="analytics-metric">
          <span>Пользователей</span>
          <strong>{stats?.users ?? '—'}</strong>
        </div>
      </div>

      <div className="analytics-chart-placeholder">
        <div className="analytics-chart-title">
          Заказы по статусам
        </div>

        {items.length ? (
          <div className="analytics-bars">
            {items.map((item) => {
              const count = Number(item.value || 0);

              const percent =
                totalOrders > 0
                  ? (count / totalOrders) * 100
                  : 0;

              return (
                <div
                  className="analytics-bar-row"
                  key={item.label}
                >
                  <span>{item.label}</span>

                  <div className="analytics-bar-track">
                    <div
                      className="analytics-bar-fill"
                      style={{
                        width: `${percent}%`,
                      }}
                    />
                  </div>

                  <strong>{count}</strong>
                </div>
              );
            })}
          </div>
        ) : (
          <div className="analytics-empty">
            Данных по заказам пока нет.
          </div>
        )}
      </div>

      {stats?.topServices?.length > 0 && (
        <div
          className="analytics-chart-placeholder"
          style={{ marginTop: '20px' }}
        >
          <div className="analytics-chart-title">
            Популярные услуги
          </div>

          <div className="analytics-bars">
            {stats.topServices.map((service) => (
              <div
                className="analytics-bar-row"
                key={service.ServiceName}
              >
                <span>
                  {service.ServiceName || 'Без названия'}
                </span>

                <div className="analytics-bar-track">
                  <div
                    className="analytics-bar-fill"
                    style={{
                      width: `${
                        totalOrders > 0
                          ? (Number(service.Count) /
                              totalOrders) *
                            100
                          : 0
                      }%`,
                    }}
                  />
                </div>

                <strong>
                  {service.Count}
                </strong>
              </div>
            ))}
          </div>
        </div>
      )}
    </section>
  );
}

export default Analytics;