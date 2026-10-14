import './HomePage.css'

function HomePage() {
  return (
    <div className="homepage">
      <header className="homepage-header">
        <h1>Фотоцентр</h1>
        <p className="subtitle">Профессиональные фотоуслуги и фототовары</p>
      </header>

      <section className="services">
        <h2>Наши услуги</h2>
        <ul>
          <li>Печать фотографий</li>
          <li>Проявление фотопленок</li>
          <li>Художественная фотосъемка</li>
          <li>Фото на документы</li>
          <li>Реставрация фотографий</li>
          <li>Выезд фотографа</li>
        </ul>
      </section>

      <section className="auth-buttons">
        <button className="btn btn-primary">Войти</button>
        <button className="btn btn-secondary">Регистрация</button>
      </section>
    </div>
  )
}

export default HomePage