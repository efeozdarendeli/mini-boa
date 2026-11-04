import { useMessaging } from '../../../messaging/useMessaging';
import { Card } from '../../../shared/components/Card';
import { PageHeader } from '../../../shared/components/PageHeader';
import { QueryBoundary } from '../../../shared/components/QueryBoundary';
import { formatCurrencyName, formatDate, formatRate } from '../../../shared/format';
import styles from './RatesView.module.css';
import { useRatesManager } from './useRatesManager';

/** Döviz kurları tablosu. Veri 1 dakika önbellekte tutulur (Client Caching). */
export function RatesView() {
  const { getMessage, language } = useMessaging();
  const manager = useRatesManager();

  return (
    <div>
      <PageHeader title={getMessage('Rates', 'Title')} onBack={manager.goBack} />
      <Card>
        <QueryBoundary query={manager.ratesQuery}>
          {(rates) => (
            <div className={styles.tableWrapper}>
              <table className={styles.table}>
                <thead>
                  <tr>
                    <th scope="col">{getMessage('Rates', 'Currency')}</th>
                    <th scope="col" className={styles.number}>
                      {getMessage('Rates', 'Buy')}
                    </th>
                    <th scope="col" className={styles.number}>
                      {getMessage('Rates', 'Sell')}
                    </th>
                    <th scope="col">{getMessage('Rates', 'Date')}</th>
                  </tr>
                </thead>
                <tbody>
                  {rates.map((rate) => (
                    <tr key={rate.currencyCode}>
                      <th scope="row">
                        <strong>{rate.currencyCode}</strong>
                        <small className={styles.currencyName}>
                          {formatCurrencyName(rate.currencyCode, language)}
                        </small>
                      </th>
                      <td className={styles.number}>{formatRate(rate.buyRate, language)}</td>
                      <td className={styles.number}>{formatRate(rate.sellRate, language)}</td>
                      <td>{formatDate(rate.rateDate, language)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </QueryBoundary>
        <p className={styles.note}>{getMessage('Rates', 'Note')}</p>
      </Card>
    </div>
  );
}
