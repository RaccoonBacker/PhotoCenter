import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { login } from '../api/api';
import './AuthModal.css';

function AuthModal({ isOpen, onClose }) {
  const navigate = useNavigate();

  const [mode, setMode] = useState('login');
  const [loginValue, setLoginValue] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);

  if (!isOpen) {
    return null;
  }

  const isLogin = mode === 'login';

  const handleSubmit = async (event) => {
    event.preventDefault();

    if (!loginValue.trim() || !password) {
      setError('Введите логин и пароль.');
      return;
    }

    setError('');
    setIsLoading(true);

    try {
      const result = await login(
        loginValue.trim(),
        password
      );

      if (result?.user?.role === 'Admin') {
        onClose();
        navigate('/admin');
        return;
      }

      onClose();
      window.location.reload();
    } catch (requestError) {
      setError(
        requestError.message ||
        'Не удалось выполнить вход.'
      );
    } finally {
      setIsLoading(false);
    }
  };

  const handleModeChange = () => {
    setMode(isLogin ? 'register' : 'login');
    setError('');
    setLoginValue('');
    setPassword('');
  };

  return (
    <div
      className="auth-modal-backdrop"
      onMouseDown={onClose}
    >
      <div
        className="auth-modal"
        role="dialog"
        aria-modal="true"
        aria-labelledby="auth-modal-title"
        onMouseDown={(event) =>
          event.stopPropagation()
        }
      >
        <button
          type="button"
          className="auth-modal-close"
          aria-label="Закрыть"
          onClick={onClose}
        >
          ×
        </button>

        <div className="auth-modal-header">
          <h2 id="auth-modal-title">
            {isLogin ? 'Вход' : 'Регистрация'}
          </h2>

          <p>
            {isLogin
              ? 'Войдите в личный кабинет фотоцентра.'
              : 'Создайте аккаунт для работы с фотоцентром.'}
          </p>
        </div>

        <form
          className="auth-form"
          onSubmit={handleSubmit}
        >
          {!isLogin && (
            <label className="auth-field">
              <span>Имя</span>

              <input
                type="text"
                name="name"
                placeholder="Введите имя"
              />
            </label>
          )}

          <label className="auth-field">
            <span>Логин</span>

            <input
              type="text"
              name="login"
              value={loginValue}
              onChange={(event) =>
                setLoginValue(event.target.value)
              }
              placeholder="Введите логин"
              autoComplete="username"
            />
          </label>

          <label className="auth-field">
            <span>Пароль</span>

            <input
              type="password"
              name="password"
              value={password}
              onChange={(event) =>
                setPassword(event.target.value)
              }
              placeholder="Введите пароль"
              autoComplete="current-password"
            />
          </label>

          {error && (
            <div
              style={{
                color: '#b42318',
                fontSize: '14px',
                marginBottom: '10px',
              }}
            >
              {error}
            </div>
          )}

          <button
            type="submit"
            className="auth-submit"
            disabled={isLoading}
          >
            {isLoading
              ? 'Выполняется вход...'
              : isLogin
                ? 'Войти'
                : 'Зарегистрироваться'}
          </button>
        </form>

        <div className="auth-switch">
          <span>
            {isLogin
              ? 'Нет аккаунта?'
              : 'Уже есть аккаунт?'}
          </span>

          <button
            type="button"
            className="auth-switch-button"
            onClick={handleModeChange}
          >
            {isLogin
              ? 'Регистрация'
              : 'Войти'}
          </button>
        </div>
      </div>
    </div>
  );
}

export default AuthModal;