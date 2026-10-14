const API_BASE = '/api';

export function getToken() {
  return localStorage.getItem('photoCenterToken');
}

export function getCurrentUser() {
  const raw = localStorage.getItem('photoCenterUser');

  if (!raw) return null;

  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

export function saveAuth(token, user) {
  localStorage.setItem(
    'photoCenterToken',
    token
  );

  localStorage.setItem(
    'photoCenterUser',
    JSON.stringify(user)
  );
}

export function clearAuth() {
  localStorage.removeItem(
    'photoCenterToken'
  );

  localStorage.removeItem(
    'photoCenterUser'
  );
}

async function request(url, options = {}) {
  const token = getToken();

  const headers = {
    ...(options.headers || {}),
  };

  if (
    options.body &&
    !headers['Content-Type']
  ) {
    headers['Content-Type'] =
      'application/json';
  }

  if (token) {
    headers.Authorization =
      `Bearer ${token}`;
  }

  const response = await fetch(
    `${API_BASE}${url}`,
    {
      ...options,
      headers,
    }
  );

  if (response.status === 401) {
    clearAuth();
  }

  if (!response.ok) {
    let message =
      `Ошибка API: ${response.status}`;

    try {
      const data =
        await response.json();

      if (data?.message) {
        message = data.message;
      } else if (data?.Message) {
        message = data.Message;
      }
    } catch {
      // Ответ не JSON.
    }

    throw new Error(message);
  }

  if (response.status === 204) {
    return null;
  }

  const text =
    await response.text();

  if (!text) {
    return null;
  }

  return JSON.parse(text);
}

export async function login(
  loginValue,
  password
) {
  const data = await request(
    '/auth/login',
    {
      method: 'POST',
      body: JSON.stringify({
        Login: loginValue,
        Password: password,
      }),
    }
  );

  saveAuth(
    data.token,
    data.user
  );

  return data;
}

export async function getUsers() {
  return request('/users');
}

export async function createUser(data) {
  return request('/users', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export async function updateUser(
  id,
  data
) {
  return request(
    `/users/${id}`,
    {
      method: 'PUT',
      body: JSON.stringify(data),
    }
  );
}

export async function deleteUser(id) {
  return request(
    `/users/${id}`,
    {
      method: 'DELETE',
    }
  );
}

export async function getOrders() {
  return request('/orders');
}

export async function createOrder(data) {
  return request('/orders', {
    method: 'POST',
    body: JSON.stringify(data),
  });
}

export async function updateOrder(
  id,
  data
) {
  return request(
    `/orders/${id}`,
    {
      method: 'PUT',
      body: JSON.stringify(data),
    }
  );
}

export async function deleteOrder(id) {
  return request(
    `/orders/${id}`,
    {
      method: 'DELETE',
    }
  );
}

export async function getAnalyticsStatuses() {
  return request(
    '/analytics/statuses'
  );
}

export async function getAnalyticsRevenue() {
  return request(
    '/analytics/revenue'
  );
}

export async function getAnalyticsTopServices() {
  return request(
    '/analytics/top-services'
  );
}