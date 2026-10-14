import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import './Header.css';
import AuthModal from './AuthModal';
import {
  clearAuth,
  getCurrentUser,
} from '../api/api';

function Header() {
  const navigate = useNavigate();
  const [isAuthOpen, setIsAuthOpen] = useState(false);
  const [currentUser, setCurrentUser] = useState(
    getCurrentUser()
  );

  const handleLogoClick = () => {
    navigate('/');
    window.scrollTo({
      top: 0,
      behavior: 'smooth',
    });
  };

  const handleHomeClick = () => {
    navigate('/');
    window.scrollTo({
      top: 0,
      behavior: 'smooth',
    });
  };

  const handleAboutClick = () => {
    navigate('/');

    setTimeout(() => {
      const el = document.getElementById('about');

      if (el) {
        el.scrollIntoView({
          behavior: 'smooth',
        });
      }
    }, 100);
  };

  const handleLogout = () => {
    clearAuth();
    setCurrentUser(null);

    if (window.location.pathname === '/admin') {
      navigate('/');
    }
  };

  const handleAuthClose = () => {
    setIsAuthOpen(false);
    setCurrentUser(getCurrentUser());
  };

  return (
    <header className="header">
      <div className="header-container">
        <div
          className="header-logo"
          onClick={handleLogoClick}
        >
          <svg
            width="32"
            height="32"
            viewBox="0 0 24 24"
            fill="none"
          >
            <circle
              cx="12"
              cy="12"
              r="10"
              stroke="currentColor"
              strokeWidth="2"
            />

            <circle
              cx="12"
              cy="12"
              r="4"
              fill="currentColor"
            />
          </svg>

          <span>Фотоцентр</span>
        </div>

        <nav className="header-nav">
          <Link
            to="/"
            className="header-link"
            onClick={handleHomeClick}
          >
            Главная
          </Link>

          <Link
            to="/services"
            className="header-link"
          >
            Услуги
          </Link>

          <Link
            to="/products"
            className="header-link"
          >
            Товары
          </Link>

          <a
            href="#about"
            className="header-link"
            onClick={handleAboutClick}
          >
            О нас
          </a>
        </nav>

        <div className="header-right">
          <Link
            to="/cart"
            className="header-cart"
          >
            🛒 <span className="cart-count">0</span>
          </Link>

          {currentUser ? (
            <>
              {currentUser.role === 'Admin' && (
                <Link
                  to="/admin"
                  className="header-login-btn"
                >
                  Админ
                </Link>
              )}

              <button
                type="button"
                className="header-login-btn"
                onClick={handleLogout}
              >
                Выйти
              </button>
            </>
          ) : (
            <button
              type="button"
              className="header-login-btn"
              onClick={() => setIsAuthOpen(true)}
            >
              Войти
            </button>
          )}
        </div>
      </div>

      <AuthModal
        isOpen={isAuthOpen}
        onClose={handleAuthClose}
      />
    </header>
  );
}

export default Header;