import { useEffect, useState } from 'react'
import { api } from '../api'
import { BarChart } from '../components/BarChart'
import { fmtKwh, fmtPln } from '../format'
import type { Dashboard } from '../types'

export function DashboardPage() {
  const [data, setData] = useState<Dashboard | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    api
      .dashboard()
      .then(setData)
      .catch((err: unknown) => setError(err instanceof Error ? err.message : 'Błąd'))
  }, [])

  if (error) {
    return <p className="error">{error}</p>
  }
  if (!data) {
    return <p className="muted">Ładowanie pulpitu…</p>
  }

  const monthDelta = data.prevMonthKwh === 0 ? 0 : ((data.monthKwh - data.prevMonthKwh) / data.prevMonthKwh) * 100

  return (
    <div className="stack">
      <header className="page-head">
        <div>
          <p className="eyebrow">Domowy monitoring</p>
          <h1>Zużycie energii</h1>
        </div>
      </header>

      {data.alerts.length > 0 && (
        <section className="alerts">
          {data.alerts.map((alert) => (
            <p key={alert}>{alert}</p>
          ))}
        </section>
      )}

      <section className="kpis">
        <article className="kpi">
          <span>Dziś</span>
          <strong>{fmtKwh(data.todayKwh)}</strong>
          <small>{fmtPln(data.todayCost)}</small>
        </article>
        <article className="kpi">
          <span>Ten miesiąc</span>
          <strong>{fmtKwh(data.monthKwh)}</strong>
          <small>
            {fmtPln(data.monthCost)} · {monthDelta >= 0 ? '+' : ''}
            {monthDelta.toFixed(0)}% vs poprz.
          </small>
        </article>
        <article className="kpi">
          <span>Szacunek urządzeń</span>
          <strong>{fmtKwh(data.deviceEstimateDailyKwh)}/d</strong>
          <small>{fmtPln(data.deviceEstimateMonthlyCost)} / 30 dni</small>
        </article>
        <article className="kpi">
          <span>Budżet</span>
          <strong>{data.budgetUsedPercent.toFixed(0)}%</strong>
          <small>
            limit {fmtKwh(data.dailyLimitKwh)}/d · {fmtPln(data.monthlyBudgetPln)}
          </small>
        </article>
      </section>

      <section className="panel">
        <div className="panel-head">
          <h2>Ostatnie 30 dni</h2>
          <small>Linia = dzienny limit</small>
        </div>
        <BarChart points={data.last30Days} limit={data.dailyLimitKwh} />
      </section>

      <section className="panel">
        <div className="panel-head">
          <h2>Największe odbiorniki</h2>
        </div>
        <table>
          <thead>
            <tr>
              <th>Urządzenie</th>
              <th>Pomieszczenie</th>
              <th>Dzień</th>
              <th>Miesiąc</th>
              <th>Koszt</th>
            </tr>
          </thead>
          <tbody>
            {data.topDevices.map((device) => (
              <tr key={`${device.roomName}-${device.name}`}>
                <td>{device.name}</td>
                <td>{device.roomName}</td>
                <td>{fmtKwh(device.dailyKwh)}</td>
                <td>{fmtKwh(device.monthlyKwh)}</td>
                <td>{fmtPln(device.monthlyCost)}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </section>
    </div>
  )
}
