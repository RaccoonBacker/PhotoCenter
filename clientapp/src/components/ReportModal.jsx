import { useEffect, useState } from 'react';
import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';

import './ReportModal.css';

const DEFAULT_LIMIT = 10;

const INITIAL_STATE = {
  users: {
    selected: true,
    all: false,
    limit: DEFAULT_LIMIT,
  },
  orders: {
    selected: true,
    all: false,
    limit: DEFAULT_LIMIT,
  },
  analytics: {
    selected: true,
  },
};

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

function formatMoney(value) {
  if (
    value === null ||
    value === undefined ||
    value === ''
  ) {
    return '—';
  }

  const number = Number(value);

  if (Number.isNaN(number)) {
    return '—';
  }

  return `${number.toFixed(2)} руб.`;
}

function normalizeLimit(value) {
  const number = Number(value);

  if (!Number.isFinite(number) || number <= 0) {
    return 1;
  }

  return Math.floor(number);
}

function ReportModal({
  isOpen,
  onClose,
  users = [],
  orders = [],
  analytics = {},
}) {
  const [settings, setSettings] = useState(
    INITIAL_STATE
  );

  const [isGenerating, setIsGenerating] =
    useState(false);

  const [error, setError] = useState('');

  useEffect(() => {
    if (isOpen) {
      setSettings(INITIAL_STATE);
      setError('');
      setIsGenerating(false);
    }
  }, [isOpen]);

  if (!isOpen) {
    return null;
  }

  const selectedCount =
    Number(settings.users.selected) +
    Number(settings.orders.selected) +
    Number(settings.analytics.selected);

  const canGenerate = selectedCount > 0;

  const handleMainCheckboxChange = (
    section,
    checked
  ) => {
    setSettings(previous => ({
      ...previous,
      [section]: {
        ...previous[section],
        selected: checked,
      },
    }));

    setError('');
  };

  const handleAllChange = (
    section,
    checked
  ) => {
    setSettings(previous => ({
      ...previous,
      [section]: {
        ...previous[section],
        all: checked,
        limit: checked
          ? previous[section].limit
          : previous[section].limit || DEFAULT_LIMIT,
      },
    }));

    setError('');
  };

  const handleLimitChange = (
    section,
    value
  ) => {
    setSettings(previous => ({
      ...previous,
      [section]: {
        ...previous[section],
        all: false,
        limit: value,
      },
    }));

    setError('');
  };

  const createSectionTitle = title => {
    const element = document.createElement('h1');

    element.className =
      'pdf-report-section-title';

    element.textContent = title;

    return element;
  };

  const createTable = (
    columns,
    rows
  ) => {
    const table =
      document.createElement('table');

    table.className = 'pdf-report-table';

    const thead =
      document.createElement('thead');

    const headerRow =
      document.createElement('tr');

    columns.forEach(column => {
      const th =
        document.createElement('th');

      th.textContent = column.label;

      headerRow.appendChild(th);
    });

    thead.appendChild(headerRow);
    table.appendChild(thead);

    const tbody =
      document.createElement('tbody');

    rows.forEach(row => {
      const tr =
        document.createElement('tr');

      columns.forEach(column => {
        const td =
          document.createElement('td');

        const value =
          typeof column.value === 'function'
            ? column.value(row)
            : row[column.value];

        td.textContent =
          value === null ||
          value === undefined ||
          value === ''
            ? '—'
            : String(value);

        tr.appendChild(td);
      });

      tbody.appendChild(tr);
    });

    table.appendChild(tbody);

    return table;
  };

  const createBarChart = (
    title,
    items,
    labelKey,
    valueKey
  ) => {
    const wrapper =
      document.createElement('div');

    wrapper.className =
      'pdf-report-chart';

    const chartTitle =
      document.createElement('h2');

    chartTitle.textContent = title;

    wrapper.appendChild(chartTitle);

    if (!items.length) {
      const empty =
        document.createElement('p');

      empty.className =
        'pdf-report-empty';

      empty.textContent =
        'Данных для построения графика нет.';

      wrapper.appendChild(empty);

      return wrapper;
    }

    const maxValue = Math.max(
      ...items.map(item =>
        Number(item[valueKey] || 0)
      ),
      1
    );

    items.forEach(item => {
      const row =
        document.createElement('div');

      row.className =
        'pdf-report-chart-row';

      const label =
        document.createElement('div');

      label.className =
        'pdf-report-chart-label';

      label.textContent =
        item[labelKey] ?? '—';

      const barArea =
        document.createElement('div');

      barArea.className =
        'pdf-report-chart-bar-area';

      const bar =
        document.createElement('div');

      bar.className =
        'pdf-report-chart-bar';

      const value =
        Number(item[valueKey] || 0);

      bar.style.width =
        `${Math.max(
          2,
          (value / maxValue) * 100
        )}%`;

      const valueLabel =
        document.createElement('span');

      valueLabel.textContent =
        String(value);

      bar.appendChild(valueLabel);
      barArea.appendChild(bar);

      row.appendChild(label);
      row.appendChild(barArea);

      wrapper.appendChild(row);
    });

    return wrapper;
  };

  const buildUsersSection = root => {
    const selectedUsers =
      settings.users.all
        ? users
        : users.slice(
            0,
            normalizeLimit(
              settings.users.limit
            )
          );

    root.appendChild(
      createSectionTitle(
        'Пользователи'
      )
    );

    const rows =
      selectedUsers.map(user => ({
        id:
          user.id ??
          user.ID,
        name:
          user.name ||
          [
            user.firstName,
            user.lastName,
          ]
            .filter(Boolean)
            .join(' ') ||
          '—',
        login:
          user.login ??
          user.Login ??
          '—',
        role: getRoleLabel(
          user.role ??
            user.Role
        ),
        status:
          user.isActive ??
          user.IsActive
            ? 'Активен'
            : 'Неактивен',
      }));

    const table = createTable(
      [
        {
          label: 'ID',
          value: 'id',
        },
        {
          label: 'Имя',
          value: 'name',
        },
        {
          label: 'Логин',
          value: 'login',
        },
        {
          label: 'Роль',
          value: 'role',
        },
        {
          label: 'Статус',
          value: 'status',
        },
      ],
      rows
    );

    root.appendChild(table);
  };

  const buildOrdersSection = root => {
    const selectedOrders =
      settings.orders.all
        ? orders
        : orders.slice(
            0,
            normalizeLimit(
              settings.orders.limit
            )
          );

    root.appendChild(
      createSectionTitle(
        'Заказы'
      )
    );

    const rows =
      selectedOrders.map(order => ({
        id:
          order.id ??
          order.ID,
        client:
          order.clientName ||
          order.ClientName ||
          '—',
        date: formatDate(
          order.createdDate ??
            order.CreatedDate
        ),
        total: formatMoney(
          order.totalPrice ??
            order.TotalPrice
        ),
        status:
          getOrderStatusLabel(
            order.status ??
              order.Status
          ),
      }));

    const table = createTable(
      [
        {
          label: 'ID',
          value: 'id',
        },
        {
          label: 'Клиент',
          value: 'client',
        },
        {
          label: 'Дата',
          value: 'date',
        },
        {
          label: 'Сумма',
          value: 'total',
        },
        {
          label: 'Статус',
          value: 'status',
        },
      ],
      rows
    );

    root.appendChild(table);
  };

  const buildAnalyticsSection = root => {
    root.appendChild(
      createSectionTitle(
        'Анализ'
      )
    );

    const statuses = Array.isArray(
      analytics.statuses
    )
      ? analytics.statuses
      : [];

    const revenue = Array.isArray(
      analytics.revenue
    )
      ? analytics.revenue
      : [];

    const topServices =
      Array.isArray(
        analytics.topServices
      )
        ? analytics.topServices
        : [];

    const statusItems =
      statuses.map(item => ({
        label:
          getOrderStatusLabel(
            item.Status
          ),
        value:
          Number(
            item.Count || 0
          ),
      }));

    const revenueItems =
      revenue.map(item => ({
        label:
          item.Month || '—',
        value:
          Number(
            item.Revenue || 0
          ),
      }));

    const serviceItems =
      topServices.map(item => ({
        label:
          item.ServiceName ||
          '—',
        value:
          Number(
            item.Count || 0
          ),
      }));

    root.appendChild(
      createBarChart(
        'Заказы по статусам',
        statusItems,
        'label',
        'value'
      )
    );

    root.appendChild(
      createBarChart(
        'Выручка по месяцам',
        revenueItems,
        'label',
        'value'
      )
    );

    root.appendChild(
      createBarChart(
        'Популярные услуги',
        serviceItems,
        'label',
        'value'
      )
    );
  };

  const generatePdf = async () => {
    if (!canGenerate || isGenerating) {
      return;
    }

    setError('');
    setIsGenerating(true);

    const pdfRoot =
      document.createElement('div');

    pdfRoot.className =
      'pdf-report-root';

    try {
      if (settings.users.selected) {
        const page =
          document.createElement('section');

        page.className =
          'pdf-report-page';

        buildUsersSection(page);

        pdfRoot.appendChild(page);
      }

      if (settings.orders.selected) {
        const page =
          document.createElement('section');

        page.className =
          'pdf-report-page';

        buildOrdersSection(page);

        pdfRoot.appendChild(page);
      }

      if (settings.analytics.selected) {
        const page =
          document.createElement('section');

        page.className =
          'pdf-report-page';

        buildAnalyticsSection(page);

        pdfRoot.appendChild(page);
      }

      document.body.appendChild(
        pdfRoot
      );

      await new Promise(resolve =>
        requestAnimationFrame(resolve)
      );

      const pages =
        Array.from(
          pdfRoot.querySelectorAll(
            '.pdf-report-page'
          )
        );

      const pdf = new jsPDF({
        orientation: 'portrait',
        unit: 'mm',
        format: 'a4',
      });

      for (
        let index = 0;
        index < pages.length;
        index += 1
      ) {
        const canvas =
          await html2canvas(
            pages[index],
            {
              scale: 2,
              useCORS: true,
              backgroundColor: '#ffffff',
            }
          );

        const imageData =
          canvas.toDataURL(
            'image/png'
          );

        const pageWidth =
          210;

        const pageHeight =
          297;

        const imageRatio =
          canvas.height /
          canvas.width;

        const imageHeight =
          pageWidth *
          imageRatio;

        let renderedHeight =
          imageHeight;

        if (
          renderedHeight >
          pageHeight
        ) {
          renderedHeight =
            pageHeight;
        }

        if (index > 0) {
          pdf.addPage();
        }

        pdf.addImage(
          imageData,
          'PNG',
          0,
          0,
          pageWidth,
          renderedHeight,
          undefined,
          'FAST'
        );
      }

      const now =
        new Date();

      const pad = value =>
        String(value).padStart(
          2,
          '0'
        );

      const fileName =
        `report_${now.getFullYear()}-${pad(
          now.getMonth() + 1
        )}-${pad(
          now.getDate()
        )}_${pad(
          now.getHours()
        )}-${pad(
          now.getMinutes()
        )}.pdf`;

      pdf.save(fileName);

      onClose();
    } catch (generationError) {
      console.error(
        'Ошибка создания PDF:',
        generationError
      );

      setError(
        'Не удалось создать PDF-отчёт.'
      );
    } finally {
      if (
        pdfRoot.parentNode
      ) {
        pdfRoot.parentNode.removeChild(
          pdfRoot
        );
      }

      setIsGenerating(false);
    }
  };

  return (
    <div
      className="report-modal-backdrop"
      onMouseDown={onClose}
    >
      <div
        className="report-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="report-modal-title"
        onMouseDown={event =>
          event.stopPropagation()
        }
      >
        <div className="report-modal-header">
          <h2 id="report-modal-title">
            Создание отчёта
          </h2>

          <button
            type="button"
            className="report-close"
            onClick={onClose}
            aria-label="Закрыть"
          >
            ×
          </button>
        </div>

        <div className="report-modal-options">
          <div className="report-option-row">
            <label className="report-main-checkbox">
              <input
                type="checkbox"
                checked={
                  settings.users.selected
                }
                onChange={event =>
                  handleMainCheckboxChange(
                    'users',
                    event.target.checked
                  )
                }
              />
              <span>
                Пользователи
              </span>
            </label>

            <label className="report-all-checkbox">
              <input
                type="checkbox"
                checked={
                  settings.users.all
                }
                onChange={event =>
                  handleAllChange(
                    'users',
                    event.target.checked
                  )
                }
              />
              <span>
                Все
              </span>
            </label>

            <input
              type="number"
              min="1"
              step="1"
              className="report-limit-input"
              value={
                settings.users.limit
              }
              disabled={
                settings.users.all
              }
              onChange={event =>
                handleLimitChange(
                  'users',
                  event.target.value
                )
              }
              aria-label="Количество пользователей"
            />
          </div>

          <div className="report-option-row">
            <label className="report-main-checkbox">
              <input
                type="checkbox"
                checked={
                  settings.orders.selected
                }
                onChange={event =>
                  handleMainCheckboxChange(
                    'orders',
                    event.target.checked
                  )
                }
              />
              <span>
                Заказы
              </span>
            </label>

            <label className="report-all-checkbox">
              <input
                type="checkbox"
                checked={
                  settings.orders.all
                }
                onChange={event =>
                  handleAllChange(
                    'orders',
                    event.target.checked
                  )
                }
              />
              <span>
                Все
              </span>
            </label>

            <input
              type="number"
              min="1"
              step="1"
              className="report-limit-input"
              value={
                settings.orders.limit
              }
              disabled={
                settings.orders.all
              }
              onChange={event =>
                handleLimitChange(
                  'orders',
                  event.target.value
                )
              }
              aria-label="Количество заказов"
            />
          </div>

          <div className="report-option-row report-analysis-row">
            <label className="report-main-checkbox">
              <input
                type="checkbox"
                checked={
                  settings.analytics.selected
                }
                onChange={event =>
                  handleMainCheckboxChange(
                    'analytics',
                    event.target.checked
                  )
                }
              />
              <span>
                Анализ
              </span>
            </label>
          </div>
        </div>

        {error && (
          <div className="report-error">
            {error}
          </div>
        )}

        <div className="report-modal-actions">
          <button
            type="button"
            className="report-secondary"
            onClick={onClose}
            disabled={isGenerating}
          >
            Отмена
          </button>

          <button
            type="button"
            className="report-primary"
            onClick={generatePdf}
            disabled={
              !canGenerate ||
              isGenerating
            }
          >
            {isGenerating
              ? 'Создание...'
              : 'Создать отчёт'}
          </button>
        </div>
      </div>
    </div>
  );
}

export default ReportModal;