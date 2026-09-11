import { useState } from 'react'
import { DashboardPage } from './pages/DashboardPage'
import { DevicesPage } from './pages/DevicesPage'
import { TariffPage } from './pages/TariffPage'
import { UsagePage } from './pages/UsagePage'
import type { Page } from './types'

const nav: { id: Page; label: string }[] = [
  { id: 'pulpit', label: 'Pulpit' },
  { id: 'urzadzenia', label: 'Urządzenia' },
  { id: 'zuzycie', label: 'Zużycie' },
  { id: 'taryfa', label: 'Taryfa' },
]

export default function App() {
  const [page, setPage] = useState<Page>('pulpit')

  return (
    <div className="shell">
      <aside>
        <div className="brand">
          <span className="mark">W</span>
          <div>
            <strong>WattDom</strong>
            <small>energia w domu</small>
          </div>
        </div>
        <nav>
          {nav.map((item) => (
            <button
              key={item.id}
              type="button"
              className={page === item.id ? 'active' : ''}
              onClick={() => setPage(item.id)}
            >
              {item.label}
            </button>
          ))}
        </nav>
      </aside>
      <main>
        {page === 'pulpit' && <DashboardPage />}
        {page === 'urzadzenia' && <DevicesPage />}
        {page === 'zuzycie' && <UsagePage />}
        {page === 'taryfa' && <TariffPage />}
      </main>
    </div>
  )
}
